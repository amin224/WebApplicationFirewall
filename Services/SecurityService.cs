using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WebApplicationFirewallUE.IRepositories;
using WebApplicationFirewallUE.IServices;
using WebApplicationFirewallUE.Models;
using Ganss.Xss;
using Newtonsoft.Json;


namespace WebApplicationFirewallUE.Services;

public class SecurityService : ISecurityService
{
    private ISecurityRepository _securityRepository;
    
    //______________DetectionSQLInjection___________________________//______________DetectionSQLInjection______________________//
    
    string inputHtml = "<p>This is a <b>test</b>. <script>alert('xss');</script> <a href='http://example.com'>Link</a></p>";
    private static readonly string[] SqlInjectionPatterns = new[]
    {
        @"--",             // SQL comment
        @"\b(select|insert|update|delete|drop|exec|create|alter|rename|truncate|declare)\b",  // SQL keywords
        @"\b(union|join|;|')\b",   // Union or SQL control characters
        @"\b(and|or)\b.*=",       // Logical operations
        @"(\%27)|(\')|(\-\-)|(\%23)|(#)", // SQL meta-characters
        @"((\%3D)|(=))[^\n]*((\%27)|(\')|(\-\-)|(\%3B)|(;))",
        @"\w*((\%27)|(\'))(\s|\%20|\s)*((\%4F)|(\%6F)|o|(\%4E)|(\%6E)|n|(\%4D)|(\%6D)|m)",
        @"\b(select|insert|update|delete|drop|exec|create|alter|rename|truncate|declare)\b",
        @"(\%28)|(\%29)|(\,)|(\%24)|(\%2B)",  // Common special characters
        @"exec(\s|\+)+(s|x)p\w+" // exec with sp_
    };

    public SecurityService(ISecurityRepository securityRepository)
    {
        _securityRepository = securityRepository;
        /*bool wasSanitized = SanitizeHtml(inputHtml, out string sanitizedHtml);*/
    }
    
    public void AnastasiaFunction(int amin)
    {
        Console.WriteLine("call me");
    }
    
    public bool ExampleSQLInjection(Users myusers)
    {
        Payload paylod = new Payload();
        return true;
    }

    public void updateUser(int userId)
    {
        _securityRepository.updateUser(userId);
    }

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
    
    
    //______________PreventingSQLInjection___________________________//______________PreventingSQLInjection______________________//

    /*using (var connection = new SqlConnection("ConnectionString"))
    {
        connection.Open();
    
        var query = "SELECT * FROM Users WHERE Username = @username AND Password = @password";
    
        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar) { Value = username });
            command.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar) { Value = password });

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Process the data
                }
            }
        }
    }*/
    //______________DetectionXSSInjection___________________________//______________DetectionXSSInjection_______________________//
    
    public bool IsXSS(string message)
    {
        // Regular expressions to detect potential XSS patterns
        string[] patterns = {
            @"<script[^>]*>.*?</script\s*>",
            @"<.*?script.*?>",
            @"onmouseover\s*=\s*(['""]).*?\1",
            @"onload\s*=\s*(['""]).*?\1",
            @"<img src=""http://url.to.file.which/not.exist"" onerror=alert(document.cookie);>",
            };

        foreach (string pattern in patterns)
        {
            if (Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                return true; // XSS pattern detected
            }
        }

        return false; // No XSS pattern detected
    }
    public bool SanitizeHtml(string message)
    {
        return true;
    }

    //______________PreventingXSSInjection___________________________//______________PreventingXSSInjection______________________//     

    
}

