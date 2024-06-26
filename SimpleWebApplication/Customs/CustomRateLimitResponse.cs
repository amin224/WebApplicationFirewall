using AspNetCoreRateLimit;
using Microsoft.Extensions.Options;

namespace SimpleWebApplication.Customs;

public class CustomRateLimitResponse : IpRateLimitMiddleware
{
    public CustomRateLimitResponse(RequestDelegate next, IProcessingStrategy processingStrategy, IOptions<IpRateLimitOptions> options, IIpPolicyStore policyStore, IRateLimitConfiguration config, ILogger<IpRateLimitMiddleware> logger) : base(next, processingStrategy, options, policyStore, config, logger)
    {
    }

    /*public override Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
    {
        if (!TimeSpan.TryParseExact(rule.Period, "m'\\m'", CultureInfo.InvariantCulture, out TimeSpan periodTimeSpan))
        {
            throw new FormatException($"Invalid TimeSpan format for period: {rule.Period}");
        }

        var retryAfterSeconds = (int)periodTimeSpan.TotalSeconds;
        
        var responseContent = new 
        {
            Message = "Custom Quota Exceeded Message",
            Details = $"Requests Exceeded Limit! Maximum allowed: {rule.Limit} per {rule.Period}. Please try again in {retryAfterSeconds} second(s)."
        };

        var responseJson = JsonSerializer.Serialize(responseContent);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = 429;
        return httpContext.Response.WriteAsync(responseJson);
    }*/
}