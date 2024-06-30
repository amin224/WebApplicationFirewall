using System.Runtime.CompilerServices;

namespace SimpleWebApplication.WebFirewall
{
    public static class CustomHeaderSecurity
    {
        public static void AddCustomHeaderAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(Settings.CustomHeaderName))
                {
                    context.Response.Headers.Append(Settings.CustomHeaderName, Settings.CustomHeaderValue);
                }
                return Task.CompletedTask;
            });
        }
    }
}