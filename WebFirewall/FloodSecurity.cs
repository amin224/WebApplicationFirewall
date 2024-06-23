using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class FloodSecurity
    {
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // write your own code here for flood protection

            // when everything is allright return true if not then return status code, response message and false this method
            return true;
        }
    }
}