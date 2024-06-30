namespace SimpleWebApplication.WebFirewall
{
    public static class Settings
    {
        public static readonly int MaxRequestsPerMin = 20;
        public static readonly TimeSpan ResetTime = TimeSpan.FromMinutes(10);
        public static readonly int DefaultBlockDurationSec = 60;
        public static readonly int DefaultExtendedBlockDurationSec = 300;
        public static readonly bool isDDoSSecurityActive = true;
        public static readonly bool isSqlInjectionSecurityActive = true;
        public static readonly bool isAntiXssSecurityActive = true;
        public static readonly bool isCsrfSecurityActive = true;
        public static readonly bool isFileInclusionSecurityActive = true;
        public static readonly bool isUserAgentFilteringSecurityActive = true;
        public const string CustomHeaderName = "MyProcess-Custom-Header";
        public const string CustomHeaderValue = "XF50SA6V?NN89B0X";
        public const string CsrfToken = "A6YD*!fV?NN89B0";
        public static readonly string[] AllowedUserAgents = {
            "Chrome/",
            "Firefox/",
            "Safari/",
            "Edge/",
            "Mozilla/5.0", // common for most modern browsers
            "Prometheus" // for real-time analysis
        };
        public static readonly string[] IgnoredPaths = {
            "/metrics", // for real-time monitoring (prometheus)
            "/css", 
            "/js", 
            "/images", 
            "/lib", 
            "/favicon.ico"
        };
    }
}
