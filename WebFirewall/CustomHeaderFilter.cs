using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebFirewall
{
    public class CustomHeaderFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check whether custom header is exists and correct
            if (context.HttpContext.Request.Headers.TryGetValue(Settings.CustomHeaderName, out var headerValue))
            {
                if (headerValue == Settings.CustomHeaderValue)
                {
                    // continue to the action if header value is correct
                    await next();
                    return;
                }
            }

            // return forbidden code and message if value is not correct
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Content = Messages.BadCustomHeaderMessage
            };
        }
    }
}
