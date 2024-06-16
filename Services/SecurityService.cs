using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplicationFirewallUE.IRepositories;
using WebApplicationFirewallUE.IServices;
using WebApplicationFirewallUE.Models;
using HtmlAgilityPack;

namespace WebApplicationFirewallUE.Services;

public class SecurityService : ISecurityService
{
    private ISecurityRepository _securityRepository;
    
    //______________SQLInjection___________________________//______________SQLInjection______________________//
    
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
    
    //______________XSSInjection___________________________//______________XSSInjection_______________________//
    
    public bool IsXSS(string message)
    {
        // Regular expressions to detect potential XSS patterns
        string[] patterns = {
            @"<script[^>]*>.*?</script\s*>",
            @"<.*?script.*?>",
            @"onmouseover\s*=\s*(['""]).*?\1",
            @"onload\s*=\s*(['""]).*?\1"
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
    
//______________HtmlSanitizer___________________________//______________HtmlSanitizer_______________________//     

    public bool SanitizeHtml(string message)
    {
        string sanitizedHtml;
        if (string.IsNullOrEmpty(message))
        {
            sanitizedHtml = string.Empty;
            return false;
        }
        //initializes a new array whitelistTags : Bold text, i: Italic text, u: Underlined text, em: Emphasized text, strong: Strongly emphasized text, a: hyperlinks
        var whitelistTags = new string[] { "b", "i", "u", "em", "strong", "a" };
        var doc = new HtmlDocument();
        doc.LoadHtml(message);

        // Remove script and style nodes
        RemoveNodes(doc, "//script|//style");

        // Remove all comments
        RemoveNodes(doc, "//comment()");

        // Remove attributes that are not safe
        RemoveUnsafeAttributes(doc);

        // Keep only whitelisted tags and their content
        SanitizeNodes(doc.DocumentNode, whitelistTags);

        sanitizedHtml = doc.DocumentNode.InnerHtml;
        var lastCheck = !sanitizedHtml.Equals(message, StringComparison.Ordinal);
        if (lastCheck == true)
            return true;

        return false;
    }

    private static void RemoveNodes(HtmlDocument doc, string xpath)
    {
        var nodes = doc.DocumentNode.SelectNodes(xpath);
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }
        }
    }

    private static void RemoveUnsafeAttributes(HtmlDocument doc)
    {
        //method SelectNodes selects all nodes and matches all elements in the document
        var nodes = doc.DocumentNode.SelectNodes("//*");
        //if the document doesn't contains any nodes, nodes will be null
        if (nodes != null)
        {
            //starts a loop to check all nodes
            foreach (var node in nodes)
            {
                //removes class, style, onclick, and any other attributes
                node.Attributes.RemoveAll();
            }
        }
    }

    private static void SanitizeNodes(HtmlNode node, string[] whitelistTags)
    {
        // checks current node is an element node
        if (node.NodeType == HtmlNodeType.Element)
        {
            // checks if current node (node.Name) is in the whitelistTags array
            if (Array.IndexOf(whitelistTags, node.Name) == -1)
            {
                // if the node is not in the whitelist, remove it but keep its content
                var parent = node.ParentNode;
                if (parent != null)
                {
                    foreach (var child in node.ChildNodes)
                    {
                        parent.InsertBefore(child, node);
                    }
                    parent.RemoveChild(node);
                }
            }
            else
            {
                // clean the attributes of whitelisted tags
                CleanAttributes(node);
            }
        }

        foreach (var childNode in node.ChildNodes)
        {
            SanitizeNodes(childNode, whitelistTags);
        }
    }

    private static void CleanAttributes(HtmlNode node)
    {
        // check node is an <a> which is a hyperlink
        if (node.Name == "a")
        {
            // get href attribute and check if it is a safe UR
            var href = node.GetAttributeValue("href", string.Empty);
            if (IsSafeUrl(href))
            {
                // remove all attributes and add back the safe href:
                node.Attributes.RemoveAll();
                node.Attributes.Add("href", href);
            }
            else
            {
                node.Attributes.RemoveAll();
            }
        }
        else
        {
            node.Attributes.RemoveAll();
        }
    }

    private static bool IsSafeUrl(string url)
    {
        // create a new Uri object from the provided URL string
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
    
}