using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebFirewall
{
    public class CSRFAttackSecurity : IAsyncActionFilter
    {
        private readonly ILogger<CSRFAttackSecurity> _logger;

        public CSRFAttackSecurity(ILogger<CSRFAttackSecurity> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.HasFormContentType ||
                !context.HttpContext.Request.Form.ContainsKey("__RequestVerificationToken"))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                _logger.LogWarning("CSRF token is missing or invalid.");
                return;
            }

            await next();
        }
    }
}