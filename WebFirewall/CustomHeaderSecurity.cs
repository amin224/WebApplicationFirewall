using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebFirewall
{
    public class CustomHeaderSecurity
    {
        // it can be used after login or some special process from web application 
        public async Task AddCustomHeaderAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(Settings.CustomHeaderName))
                {
                    context.Response.Headers.Add(Settings.CustomHeaderName, Settings.CustomHeaderValue);
                }
                return Task.CompletedTask;
            });
        }
    }
}