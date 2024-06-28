using Microsoft.AspNetCore.Mvc;

namespace SimpleWebApplication.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult CustomErrorPage()
        {
            // fetching the error message from temp data
            if (HttpContext.Items["Exception"] is string errorMessage)
            {
                ViewData["ErrorMessage"] = errorMessage;
            }
            else
            {
                ViewData["ErrorMessage"] = "An unknown error occurred.";
            }

            return View();
        }
    }
}