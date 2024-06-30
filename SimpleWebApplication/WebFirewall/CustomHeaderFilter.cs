﻿using Audit.WebApi;
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

        public CustomHeaderFilter(AuditConfiguration auditConfiguration)
        {
            _auditConfiguration = auditConfiguration;
        }

        [AuditApi]
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Headers.TryGetValue(Settings.CustomHeaderName, out var headerValue))
            {
                if (headerValue == Settings.CustomHeaderValue)
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