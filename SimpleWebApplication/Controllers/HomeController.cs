using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.Models;
using System.Diagnostics;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;
using Npgsql;
using System.Data.Common;
using System.Text;
using System.Security.Cryptography;
using SimpleWebApplication.Engines;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SimpleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailEngine _emailEngine;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IEmailEngine emailEngine)
        {
            _logger = logger;
            _configuration = configuration;
            _emailEngine = emailEngine;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Documents()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SendDocument()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendDocument(IFormFile file)
        {
            Console.Write("true");
            return Ok();
        }
    }
}