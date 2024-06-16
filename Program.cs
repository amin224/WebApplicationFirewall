using WebApplicationFirewallUE.IRepositories;
using WebApplicationFirewallUE.IServices;
using WebApplicationFirewallUE.Repositories;
using WebApplicationFirewallUE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Add controllers to the services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ISecurityService, SecurityService>();
builder.Services.AddSingleton<ISecurityRepository, SecurityRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Map controller routes
app.Run();
