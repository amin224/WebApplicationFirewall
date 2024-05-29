using Microsoft.AspNetCore.Mvc;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;
    private readonly IConfiguration _configuration;

    public SecurityController(ILogger<SecurityController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [Route("GETSend")]
    public Task<ActionResult<Payload>> VerifyRequestIntegrity()
    {
        return null;
    }
}