using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class XssSecurity
    {
        // Some of common XSS attack patterns
        private static readonly string[] XssPatterns = 
        {
            @"javascript:",
            @"onerror\s*=\s*",
            @"onload\s*=\s*",
            @"<\s*img\s+src\s*=\s*[^\s>]+>",
            @"<\s*a\s+href\s*=\s*[^\s>]+>",
            @"<\s*iframe\s+[^<]*<\s*\/\s*iframe\s*>",
            @"<\s*link\s+[^<]*<\s*\/\s*link\s*>",
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>"
        };

        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            // Checking  query parameters
            if (ContainsXss(context.Request.QueryString.Value))
            {
                await BlockRequestAsync(context, Messages.XssBanned);
                return false;
            }

            // Check form data (if any)
            if (context.Request.HasFormContentType && context.Request.Form != null)
            {
                foreach (var key in context.Request.Form.Keys)
                {
                    if (ContainsXss(context.Request.Form[key]))
                    {
                        await BlockRequestAsync(context, Messages.XssBanned);
                        return false;
                    }
                }
            }

            // Check headers
            foreach (var header in context.Request.Headers)
            {
                if (ContainsXss(header.Value.ToString()))
                {
                    await BlockRequestAsync(context, Messages.XssBanned);
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsXss(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            foreach (var pattern in XssPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        private static async Task BlockRequestAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(message);
        }
    }
}
