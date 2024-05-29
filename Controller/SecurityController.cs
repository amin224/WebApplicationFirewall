using Microsoft.AspNetCore.Mvc;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.Controller;

[Route("api/[controller]")]
[ApiController]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(ILogger<SecurityController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("GETSend")]
    public Task<ActionResult<Payload>> VerifyRequestIntegrity()
    {
        return null;
    }
}