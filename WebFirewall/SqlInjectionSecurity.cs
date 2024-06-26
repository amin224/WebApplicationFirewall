using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class SqlInjectionSecurity
    {
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
        
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            // write your own SQL injection protection logic here
            // For example, check request parameters includes sql injection pattern
            var input = context.Request.QueryString.Value;
            if (string.IsNullOrEmpty(input))
                return true;

            // Example logic for blocking
            // note: you can use a array which stores possible dangerous inputs instead of just checking one text
            /*if (context.Request.QueryString.Value.Contains("DROP TABLE"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(Messages.SqlInjectionBanned);
                return true;
            }*/
            
            foreach (var pattern in SqlInjectionPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(Messages.SqlInjectionBanned);
                return true;
            }

            return false;
        }
    }
}