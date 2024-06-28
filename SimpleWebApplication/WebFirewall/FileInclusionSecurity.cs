using System.Net;
using System.Text.RegularExpressions;
using Audit.WebApi;
using nClam;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Models;

namespace WebFirewall
{
    public partial class FileInclusionSecurity
    {
        private readonly AuditConfiguration _auditConfiguration;
        private static readonly HttpClient HttpClient = new();
        private static readonly string[] TrustedDomains = { "github.com" };
        private static readonly string[] MaliciousPatterns =
            { "<script>", "<?php", "eval(", "exec(", "system(", "'; DROP TABLE", "<!--#exec" };
        private static readonly string[] AllowedFileExtensions = { ".txt", ".pdf" };

        public FileInclusionSecurity(AuditConfiguration auditConfiguration)
        {
            _auditConfiguration = auditConfiguration;
        }

        [AuditApi]
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            // checking whether the request method supports from files (ex:POST)
            if (context.Request.Method != HttpMethods.Post)
            {
                return true;
            }

            // Checking whether the request has form data
            if (!context.Request.HasFormContentType)
            {
                return true;
            }

            // Checking whether there are files in the form
            if (context.Request.Form.Files.Count == 0)
            {
                return true;
            }

            var formFile = context.Request.Form.Files[0];

            if (formFile == null)
            {
                return true;
            }

            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                using (var reader = new StreamReader(memoryStream, leaveOpen: true))
                {
                    var content = await reader.ReadToEndAsync();
                    if (MyRegex().IsMatch(content))
                    {
                        // Log the file inclusion attack
                        var log = new LogTraceOperation(true, "FileInclusion");
                        _auditConfiguration.AuditCustomFields(log);

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync(Messages.FileInclusionBanned);

                        return false;
                    }
                }

                memoryStream.Position = 0;
                var isSafe = await ClamAvScanAsync(memoryStream);
                return isSafe;
            }
        }

        private async Task<bool> ClamAvScanAsync(Stream fileStream)
        {
            try
            {
                var clamClient = new ClamClient(IPAddress.Parse("127.0.0.1"));
                var result = await clamClient.SendAndScanFileAsync(fileStream);
                if (result.InfectedFiles != null || result.InfectedFiles.Count != 0)
                {
                    // Log the file inclusion attack
                    var log = new LogTraceOperation(true, "FileInclusion");
                    _auditConfiguration.AuditCustomFields(log);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error scanning file with ClamAV: {ex.Message}");
            }
        }
        
        public async Task<bool> SafeRemoteFileInclude(string remoteUrl)
        {
            if (!Uri.TryCreate(remoteUrl, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps ||
                !TrustedDomains.Any(domain => uri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
 
            var rawUrl = remoteUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
            var fileContent = await HttpClient.GetStringAsync(rawUrl);

            if (MaliciousPatterns.Any(fileContent.Contains))
            {
                return false;
            }        
 
            return AllowedFileExtensions.Contains(Path.GetExtension(uri.LocalPath).ToLower());
        }
        
        [GeneratedRegex(@"<script\b[^>]*>([\s\S]*?)<\/script>", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex();
    }
}