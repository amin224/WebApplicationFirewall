using System.IO.Abstractions;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimpleWebApplication.Customs;
using SimpleWebApplication.Engines;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Repositories;
using SimpleWebApplication.WebFirewall;
using StackExchange.Redis;

var isRateLimitingActive = false;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddDbContext<MyAppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyAppDbConnection")));

// var redisConfiguration = builder.Configuration["Redis:Configuration"];
// var redis = ConnectionMultiplexer.Connect(redisConfiguration);

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = redisConfiguration;
//});

// builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddControllersWithViews();

if (isRateLimitingActive)
{
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfigurationService>();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

    builder.Services.AddSingleton<IClientResolveContributor, CustomClientResolver>();
    builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
}

builder.Services.AddControllers(configure =>
{
    AuditConfiguration.AddAudit((configure));
    AuditConfiguration.ConfigureAudit(builder.Services);
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<AuditConfiguration, AuditConfiguration>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<IEmailEngine, EmailEngine>();

// builder.Services.AddScoped<CustomHeaderFilter>(provider =>
//{
//    var auditConfiguration = provider.GetRequiredService<AuditConfiguration>();
//    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
//    var httpContext = httpContextAccessor.HttpContext;

//    string headerName = "CustomHeader";
//    string headerValue = string.Empty;

//    if (httpContext != null)
//    {
//        if (httpContext.Request.Path.Value.Contains("MyProcess"))
//        {
//            headerValue = "Pending";
//        }
//        else if (httpContext.Request.Path.Value.Contains("TransferMoney"))
//        {
//        }
//    }

//    return new CustomHeaderFilter(auditConfiguration, headerName, headerValue);
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/User/Login";
})
.AddJwtBearer("WebFirewall", options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            builder.Configuration["JwtSettings:SecretKey"])),
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseCookiePolicy();

// web firewall
if (isRateLimitingActive)
{
    app.UseMiddleware<CustomRateLimitResponse>();

    using (var scope = app.Services.CreateScope())
    {
        var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
        await ipPolicyStore.SeedAsync();
    }
}

// web firewall
app.UseMiddleware<Init>();

app.UseCors(options =>
{
    options.WithMethods("GET", "POST")
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
