using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WebFirewall
{
    internal static class Messages
    {
        internal static string BannedMessage = "Your IP address is banned";
        internal static string CsrfTokenNotFound = "CSRF token not found.";
        internal static string SqlInjectionBannedMessage = "Sql injection attack detected.";
        internal static string XssBannedMessage = "Upss XSS attack detected";
        internal static string BadCustomHeaderMessage = "Custom header is not valid";
    }
}
