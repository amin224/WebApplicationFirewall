using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class SqlInjectionSecurity
    {
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            // write your own SQL injection protection logic here
            // For example, check request parameters includes sql injection pattern

            // Example logic for blocking
            // note: you can use a array which stores possible dangerous inputs instead of just checking one text
            if (context.Request.QueryString.Value.Contains("DROP TABLE"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(Messages.SqlInjectionBannedMessage);
                return false;
            }

            return true;
        }
    }
}