using Microsoft.AspNetCore.Authorization;
using WebApplicationFirewallUE.IServices;

namespace WebApplicationFirewallUE.Services;

[Authorize(AuthenticationSchemes = "WebFirewall")]
public class RateLimitService : IRateLimitService
{
    
}