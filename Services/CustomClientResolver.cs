using AspNetCoreRateLimit;

namespace WebApplicationFirewallUE.Services;

public class CustomClientResolver : IClientResolveContributor
{
    
    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        var clientId = httpContext.Request.Headers["X-Client-Id"].ToString();

        return Task.FromResult(clientId);
    }
}