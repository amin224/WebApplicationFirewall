using System.Text.RegularExpressions;
using Ganss.Xss;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Security.Application;
using WebApplicationFirewallUE.IServices;
using WebApplicationFirewallUE.Models;

namespace WebApplicationFirewallUE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SecurityController : ControllerBase
{
    private readonly ILogger<SecurityController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ISecurityService _securityService;

    public SecurityController(ILogger<SecurityController> logger, IConfiguration configuration,
        ISecurityService securityService)
    {
        _logger = logger;
        _configuration = configuration;
        _securityService = securityService;
    }
  
    [HttpGet]
    [Route("index")]
    public async Task<ActionResult<Users>> VerifyRequestIntegrity()
    {
        var user = new Users
        {
            Name = "Nastya",
            Id = 1
        };
        return user;

    }

    [HttpGet("anastasiaAmin")]
    public IActionResult CheckXSS([FromQuery] string message)
    {
        bool storeVariableResult = _securityService.IsXSS(message);
        Console.WriteLine("Result of Check XSS is " + storeVariableResult);
        if (storeVariableResult == true)
        {
            var result = _securityService.SanitizeHtml(message);
            if (result)
                return Ok(true);
            return Ok(false);
        }

        return Ok(false);
    }
    
    [HttpGet("data")]
    public IActionResult GetData()
    {
        var data = new
        {
            Message = "Hello from the API!",
            Timestamp = DateTime.UtcNow
        };

        return Ok(data);
    }
    
    [HttpGet("check")]
    public IActionResult CheckSql([FromQuery] string message)
    {
        //string result = "' OR '1'='1";
        Console.WriteLine("My received message was: " + message);
        var callFromService = _securityService.IsSqlInjection(message);
        
        Console.WriteLine("The request for SQL Injection is: " + callFromService);

        return Ok(callFromService);
    }
    
    
    [HttpPost("check1")]
    public ActionResult SubmitData(string inputData)
    {
        // Process the inputData here
        Console.WriteLine ("Data submitted successfully!");
        return Ok();;
    }

    
    [HttpGet("checkXSS")]
    public ActionResult CheckXssPayload([FromQuery] string message)
    {
        //receive message
        //so depending on the message you have to check which context and encode for it
        //if (string.IsNullOrEmpty(message))
        //{
            //return BadRequest("Message is empty");
        //}
        // Declare the encodedString variable
        string encodedString;
        
        //check tags and see which type is it html, javascript, url, html attribute
        //Encode the payload to prevent XSS attacks
        if (Regex.IsMatch(message, "<.*?>"))
        {
            //tags for html send to function to check htmlEncode
            
            encodedString = Encoder.HtmlEncode(message);
        }
        else if (Regex.IsMatch(message, @"^https?:\/\/")) 
        {
            //tags for html send to function to check URL context
            encodedString = Encoder.UrlEncode(message);
        }
        else if (Regex.IsMatch(message, @"[;{}()=]"))
        {
            //tags for javascript send to function to check javascriptEncode
            encodedString = Encoder.JavaScriptEncode(message);
        }
        else
        {
            // Default to HTML encoding
            encodedString = Encoder.HtmlEncode(message);
        }
        // Return the encoded payload
        return Ok(encodedString);
    }
}  
    
    
    //[HttpGet("sanitize")]
    //public string SanitizeInput([FromQuery] string html)
    
