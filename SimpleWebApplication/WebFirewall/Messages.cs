namespace SimpleWebApplication.WebFirewall
{
    public static class Messages
    {
        public static string Banned = "Your IP address is banned";
        internal static string CsrfTokenNotFound = "CSRF token not found.";
        internal static string SqlInjectionBanned = "Sql injection attack detected.";
        internal static string FileInclusionBanned = "File inclusion attack detected";
        internal static string XssBanned = "Upss XSS attack detected";
        internal static string BadCustomHeader = "You are not allowed to see this page";
        public static string DeniedBrowser = "Your browser is not allowed.";
        internal static string Error = "There is an internal error. Error: {0}";
    }
}
