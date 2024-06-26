using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using nClam;

namespace WebFirewall
{
    public partial class FileInclusionSecurity
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly string[] TrustedDomains = { "github.com" };
        private static readonly string[] MaliciousPatterns =
            { "<script>", "<?php", "eval(", "exec(", "system(", "'; DROP TABLE", "<!--#exec" };
        private static readonly string[] AllowedFileExtensions = { ".txt", ".pdf" };
        
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            try
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                if (context.Request.Form.Files.Count == 0)
                {
                    return false;
                }

                var formFile = context.Request.Form.Files[0];
                
                if (formFile == null)
                {
                    return false;
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
                            return false;
                        }
                    }

                    memoryStream.Position = 0;
                    var isSafe = ClamAvScanAsync(memoryStream);
                    return await isSafe;
                }

                // write your own code here for file inclusion protection

                // when everything is allright return true if not then return status code, response message and false this method
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ClamAvScanAsync(Stream fileStream)
        {
            try
            {
                var clamClient = new ClamClient(IPAddress.Parse("127.0.0.1"));
                var result = await clamClient.SendAndScanFileAsync(fileStream);
                return result.InfectedFiles == null || result.InfectedFiles.Count == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning file with ClamAV: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> SafeRemoteFileInclude(string remoteUrl)
        {
            if (!Uri.TryCreate(remoteUrl, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps ||
                !TrustedDomains.Any(domain => uri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase)))
                return false;
 
            var rawUrl = remoteUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
            var fileContent = await HttpClient.GetStringAsync(rawUrl);

            if (MaliciousPatterns.Any(fileContent.Contains))
                return false;
 
            return AllowedFileExtensions.Contains(Path.GetExtension(uri.LocalPath).ToLower());
        }
        
        [GeneratedRegex(@"<script\b[^>]*>([\s\S]*?)<\/script>", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MyRegex();
    }
}