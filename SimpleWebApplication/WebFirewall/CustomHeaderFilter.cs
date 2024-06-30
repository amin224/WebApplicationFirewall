using Audit.WebApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Models;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SimpleWebApplication.WebFirewall
{
    public class CustomHeaderFilter : IAsyncActionFilter
    {
        private readonly AuditConfiguration _auditConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _headerName;
        private readonly string _headerValue;

        public CustomHeaderFilter(AuditConfiguration auditConfiguration, string headerName, string headerValue)
        {
            _auditConfiguration = auditConfiguration;
            _headerName = headerName;
            _headerValue = headerValue;
        }

        [AuditApi]
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check whether custom header is exists and correct
            //var httpContext = _httpContextAccessor.HttpContext;
            //string headerName = Settings.CustomHeaderName;
            //string headerValue = Settings.CustomHeaderValue;

            //if (httpContext.Request.Path.Value.Contains("MyProcess"))
            //{
            //    headerName = "MyProcess";
            //    headerValue = "Pending";
            //}
            //else if (httpContext.Request.Path.Value.Contains("MoneyTransfer"))
            //{
            //    headerName = "MoneyTransfer";
            //    headerValue = "Approved";
            //}


            //if (context.HttpContext.Request.Headers.TryGetValue(headerName, out var actualHeaderValue))
            //{
            //    if (actualHeaderValue == headerValue)
            //    {
            //        await next();
            //        return;
            //    }
            //}

            if (context.HttpContext.Request.Headers.TryGetValue(_headerName, out var actualHeaderValue))
            {
                if (actualHeaderValue == _headerValue)
                {
                    await next();
                    return;
                }
            }

            // Log the custom header checking is failed
            var log = new LogTraceOperation(true, "CustomHeaderVerification");
            _auditConfiguration.AuditCustomFields(log);

            // return forbidden code and message if value is not correct
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Content = Messages.BadCustomHeader
            };
        }
    }
}