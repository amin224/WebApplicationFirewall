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
        internal static string Banned = "Your IP address is banned";
        internal static string CsrfTokenNotFound = "CSRF token not found.";
        internal static string SqlInjectionBanned = "Sql injection attack detected.";
        internal static string XssBanned = "Upss XSS attack detected";
        internal static string BadCustomHeader = "Custom header is not valid";
        internal static string DeniedBrowser = "Your browser is not allowed.";
        internal static string Error = "There is an internal error. Try again later";
    }
}
