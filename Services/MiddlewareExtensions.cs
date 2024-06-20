namespace WebApplicationFirewallUE.Services;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAntiXssMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AntiXssMiddleware>();
    }
}