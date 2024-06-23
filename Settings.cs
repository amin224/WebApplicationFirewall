using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackerTool
{
    public static class Settings
    {
        public const int DefaultLimit = 20;

        public static readonly string[] UserAgents =
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0"
        };

        public static readonly string[] Proxies = 
        {
            "http://proxy1.proxyserver.com:8080",
            "http://proxy2.proxyserver.com:8080",
            "http://proxy3.proxyserver.com:8080",
            "http://proxy4.proxyserver.com:8080"
        };

        public static readonly List<string> LoopbackIPs = new()
        {
            "127.0.0.1", "127.0.0.2", "127.0.0.3", "127.0.0.4"
        };

        public static List<HttpStatusCode> BlockedStatusCode = new List<HttpStatusCode>()
        {
            HttpStatusCode.Forbidden,
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.Unauthorized
        };

        public static List<HttpStatusCode> ServerDownCode = new List<HttpStatusCode>()
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable
        };
    }
}