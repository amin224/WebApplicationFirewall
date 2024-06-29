using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.Models;
using System.Diagnostics;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;

namespace SimpleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // this line will be moved under login post action. it is here as temp
            CustomHeaderSecurity.AddCustomHeaderAsync(HttpContext);
            return View();
        }

        [ServiceFilter(typeof(CustomHeaderFilter))]
        public IActionResult PrivatePage()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
    }
}
