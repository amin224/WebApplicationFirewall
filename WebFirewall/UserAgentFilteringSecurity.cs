using Microsoft.AspNetCore.Http;

namespace WebFirewall
{
    public class UserAgentFilteringSecurity
    {
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (!Settings.AllowedUserAgents.Any(ua => userAgent.Contains(ua)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(Messages.DeniedBrowser);
                return false;
            }

            return true;
        }
    }
}
