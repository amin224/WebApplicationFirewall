using System.IO.Abstractions;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using WebApplicationFirewallUE.IRepositories;
using WebApplicationFirewallUE.IServices;
using WebApplicationFirewallUE.Models;
using WebApplicationFirewallUE.Repositories;
using WebApplicationFirewallUE.Services;
using WebApplicationFirewallUE.Static;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddDbContext<MyAppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyAppDbConnection")));

var redisConfiguration = builder.Configuration["Redis:Configuration"];
var redis = ConnectionMultiplexer.Connect(redisConfiguration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfiguration;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfigurationService>();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

builder.Services.AddSingleton<IClientResolveContributor, CustomClientResolver>();
builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

builder.Services.Configure<FileUploadInfo>(
    builder.Configuration.GetSection("FileUploadInfo"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(configure =>
{
    AuditConfiguration.AddAudit((configure));
    AuditConfiguration.ConfigureAudit(builder.Services);
});
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ISecurityService, SecurityService>();
builder.Services.AddSingleton<ISecurityRepository, SecurityRepository>();
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
builder.Services.AddSingleton<AuditConfiguration, AuditConfiguration>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IFileInclusionService, FileInclusionService>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
/*builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();*/

builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
                "P/qVjn2qCz9yZqOebHMZyb0AS0WDTSKl1gV76NCVUcHxOrm+m93Zfen5i0Apx4XGhOPflfXxd7USrkF0zPOm5g==")),
            ValidIssuer = "Identity",
            ValidAudience = "WebFirewall",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseIpRateLimiting();
app.UseMiddleware<CustomRateLimitResponse>();
app.UseCors(options =>
{
    options.AllowAnyMethod()
        .AllowAnyOrigin()
        .AllowAnyHeader();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
    await ipPolicyStore.SeedAsync();
}

app.Run();