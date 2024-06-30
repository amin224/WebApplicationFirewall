using System.Runtime.CompilerServices;

namespace SimpleWebApplication.WebFirewall
{
    public static class CustomHeaderSecurity
    {
        public static void AddCustomHeaderAsync(HttpContext context, string headerName, string headerValue)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(headerName))
                {
                    context.Response.Headers.Append(headerName, headerValue);
                }
                return Task.CompletedTask;
            });
        }
    }
}