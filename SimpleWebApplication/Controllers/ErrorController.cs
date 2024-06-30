using Microsoft.AspNetCore.Mvc;

namespace SimpleWebApplication.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult CustomErrorPage()
        {
            var errorMessage = HttpContext.Items["Exception"]?.ToString();
            ViewData["ErrorMessage"] = errorMessage;
            return View();
        }
    }
}