using Audit.WebApi;
using Microsoft.AspNetCore.Http;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class DDoSSecurity
    {
        private readonly AuditConfiguration _auditConfiguration;
        private static readonly ConcurrentDictionary<string, ClientRequest> _clientRequest = new ConcurrentDictionary<string, ClientRequest>();

        public DDoSSecurity(AuditConfiguration auditConfiguration)
        {
            _auditConfiguration = auditConfiguration;
        }

        [AuditApi]
        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;

            // Add new client or update existing client request
            _clientRequest.AddOrUpdate(clientIp,
                new ClientRequest(1, now, 0),
                (key, existingClientRequest) => UpdateClientRequest(existingClientRequest, now));

            ClientRequest clientRequest = _clientRequest[clientIp];

            // Check if the client is currently blocked
            if (IsClientBlocked(clientRequest, now))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(Messages.Banned);
                return false;
            }

            // Check if the request count exceeds the limit
            if (clientRequest.RequestCount > Settings.MaxRequestsPerMin)
            {
                // Log the ddos-flood attack
                var log = new LogTraceOperation(true, "DDoS-Flood");
                _auditConfiguration.AuditCustomFields(log);

                // Set block duration and update the last request time
                clientRequest.BlockDuration = clientRequest.BlockDuration == 0 ?
                    Settings.DefaultBlockDurationSec : Settings.DefaultExtendedBlockDurationSec;
                clientRequest.LastRequestTime = now;

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(Messages.Banned);
                return false;
            }

            return true;
        }

        // Update client request: reset if needed, increment request count, update last request time
        private ClientRequest UpdateClientRequest(ClientRequest clientRequest, DateTime now)
        {
            if (ShouldResetRequestCount(clientRequest, now))
            {
                ResetClientRequest(clientRequest, now);
            }

            clientRequest.RequestCount++;
            clientRequest.LastRequestTime = now;

            return clientRequest;
        }

        // Check if the client is within the block period
        private static bool IsClientBlocked(ClientRequest clientRequest, DateTime now)
        {
            return now < clientRequest.LastRequestTime.AddSeconds(clientRequest.BlockDuration);
        }

        // Check if the reset time has passed
        private static bool ShouldResetRequestCount(ClientRequest clientRequest, DateTime now)
        {
            return now >= clientRequest.LastRequestTime.Add(Settings.ResetTime);
        }

        // Reset client request parameters
        private static void ResetClientRequest(ClientRequest clientRequest, DateTime now)
        {
            clientRequest.RequestCount = 0;
            clientRequest.BlockDuration = 0;
            clientRequest.LastRequestTime = now;
        }
    }
}