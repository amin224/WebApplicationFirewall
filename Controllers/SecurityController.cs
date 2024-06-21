using Audit.WebApi;
using Microsoft.AspNetCore.Mvc;
using WebApplicationFirewallUE.IServices;
using WebApplicationFirewallUE.Models;
using WebApplicationFirewallUE.Static;

namespace WebApplicationFirewallUE.Controllers;

[ApiController]
[Route("api/[controller]")]
[AutoValidateAntiforgeryToken]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;
    private readonly IConfiguration _configuration;
    private readonly AuditConfiguration _auditConfiguration;
    private readonly IFileInclusionService _fileInclusionService;

    public SecurityController(ILogger<SecurityController> logger, IConfiguration configuration, 
        AuditConfiguration auditConfiguration, IFileInclusionService fileInclusionService)
    {
        _logger = logger;
        _configuration = configuration;
        _auditConfiguration = auditConfiguration;
        _fileInclusionService = fileInclusionService;
    }

    [HttpGet]
    [Route("GETSend")]
    public Task<ActionResult<Payload>> VerifyRequestIntegrity()
    {
        return null;
    }
    
    [HttpPost]
    [Route("GenerateToken")]
    public IActionResult GenerateToken([FromBody] LoginRequest loginRequest)
    {
        var textPassword = _configuration["JwtSettings:Password"];
        
        if (loginRequest.Username != _configuration["JwtSettings:Username"] || !VerifyPassword(textPassword!, loginRequest.Password))
            return Unauthorized("Invalid credentials.");
        
        var authHelper = new AuthorizationHelper(_configuration);
        var token = authHelper.GenerateJwtToken(loginRequest.Username);
        
        return Ok(token);
    }
    
    private bool VerifyPassword(string inputPassword, string storedHashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedHashedPassword);
    }

    [HttpGet]
    [Route("TestRateLimit")]
    [AuditApi]
    public bool TestRate()
    {
        var logTrace = new LogTraceOperation(true, "SQLInjection");
        Console.WriteLine("true");
        _auditConfiguration.AuditCustomFields(logTrace);
        return true;
    }
    
    [HttpPost("upload")]
    public IActionResult UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");
        
        var fileExtension = Path.GetExtension(file.FileName);
        var allowedExtension = _configuration["FileUploadSettings:AllowedExtensions"];
        if (!allowedExtension.Contains(fileExtension))
            return BadRequest("File type not allowed.");
        
        var result = _fileInclusionService.CheckFileInclusion(file);
        if(!result.Result) 
            return BadRequest(result.Result);
        
        return Ok(result.Result);
    }
    
    [HttpPost("UrlInput")]
    public IActionResult UrlInputCheck(string remoteUrl)
    {
        var checkUrl = _fileInclusionService.SafeRemoteFileInclude(remoteUrl);
        
        return Ok(checkUrl.Result);
    }
    
    
}