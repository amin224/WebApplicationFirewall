using System.Text.RegularExpressions;
using WebApplicationFirewallUE.IServices;

namespace WebApplicationFirewallUE.Services;

public class SecurityService : ISecurityService
{
    private string _inputHtml = "<p>This is a <b>test</b>. <script>alert('xss');</script> <a href='http://example.com'>Link</a></p>";
    private static readonly string[] SqlInjectionPatterns = new[]
    {
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
    };
    
    public bool IsSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        foreach (var pattern in SqlInjectionPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }
    
    public bool IsXss(string message)
    {
        string[] patterns =
        [
            @"<script[^>]*>.*?</script\s*>",
            @"<.*?script.*?>",
            @"onmouseover\s*=\s*(['""]).*?\1",
            @"onload\s*=\s*(['""]).*?\1",
            @"<img src=""http://url.to.file.which/not.exist"" onerror=alert(document.cookie);>"
        ];

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                return true;
            }
        }

        return false;
    }
    
    
}