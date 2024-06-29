using Audit.WebApi;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.WebFirewall
{
    public class UserAgentFilteringSecurity
    {
        private readonly AuditConfiguration _auditConfiguration;

        public UserAgentFilteringSecurity(AuditConfiguration auditConfiguration)
        {
            _auditConfiguration = auditConfiguration;
        }

        [AuditApi]
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (!Settings.AllowedUserAgents.Any(ua => userAgent.Contains(ua)))
            {
                // Log user agent is not allowed
                var log = new LogTraceOperation(true, "UserAgentFiltering");
                _auditConfiguration.AuditCustomFields(log);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(Messages.DeniedBrowser);

                return false;
            }

            return true;
        }
    }
}
