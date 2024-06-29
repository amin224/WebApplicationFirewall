using System.Text.RegularExpressions;
using Audit.WebApi;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.WebFirewall
{
    public class SqlInjectionSecurity
    {
        private readonly AuditConfiguration _auditConfiguration;

        public SqlInjectionSecurity(AuditConfiguration auditConfiguration)
        {
            _auditConfiguration = auditConfiguration;
        }

        private string _inputHtml = "<p>This is a <b>test</b>. <script>alert('xss');</script> <a href='http://example.com'>Link</a></p>";
        private static readonly string[] SqlInjectionPatterns =
        [
            @"--",
            @"\b(select|insert|update|delete|drop|exec|create|alter|rename|truncate|declare)\b",
            @"\b(union|join|;|')\b",
            @"\b(and|or)\b.*=",
            @"(\%27)|(\')|(\-\-)|(\%23)|(#)",
            @"((\%3D)|(=))[^\n]*((\%27)|(\')|(\-\-)|(\%3B)|(;))",
            @"\w*((\%27)|(\'))(\s|\%20|\s)*((\%4F)|(\%6F)|o|(\%4E)|(\%6E)|n|(\%4D)|(\%6D)|m)",
            @"\b(select|insert|update|delete|drop|exec|create|alter|rename|truncate|declare)\b",
            @"(\%28)|(\%29)|(\,)|(\%24)|(\%2B)",
            @"exec(\s|\+)+(s|x)p\w+"
        ];

        [AuditApi]
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            var input = context.Request.QueryString.Value;
            if (string.IsNullOrEmpty(input)) return true;

            // check request parameters includes sql injection pattern
            foreach (var pattern in SqlInjectionPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                {
                    // Log sql injection attack
                    var log = new LogTraceOperation(true, "SQLInjection");
                    _auditConfiguration.AuditCustomFields(log);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync(Messages.SqlInjectionBanned);
                    
                    return false;
                }
            }

            return true;
        }
    }
}