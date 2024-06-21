using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class DDoSSecurity
    {
        private static readonly ConcurrentDictionary<string, ClientRequest> _clientRequest = new ConcurrentDictionary<string, ClientRequest>();

        public async Task<bool> CheckRequestAsync(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;

            if (!_clientRequest.TryGetValue(clientIp, out var clientRequest))
            {
                clientRequest = new ClientRequest(1, now, 0);
                _clientRequest[clientIp] = clientRequest;
            }
            else
            {
                // Checking whether the client is already blocked
                if (IsClientBlocked(clientRequest, now))
                {
                    await BlockClientAsync(context);
                    return false;
                }

                // Reset count for this ip if the reset time has passed
                if (ShouldResetRequestCount(clientRequest, now))
                {
                    ResetClientRequest(clientRequest, now);
                }

                // check the request count is over certain time limit
                if (clientRequest.RequestCount > Settings.MaxRequestsPerMin)
                {
                    // Set duration time for blocking and update the last request time
                    clientRequest.BlockDuration = Settings.DefaultBlockDurationSec;
                    clientRequest.LastRequestTime = now;
                    await BlockClientAsync(context);
                    return false;
                }

                // Incrementing each request for same ip
                clientRequest.RequestCount++;

                // Update latest request time
                clientRequest.LastRequestTime = now;
            }

            return true;
        }

        private static bool IsClientBlocked(ClientRequest clientRequest, DateTime now)
        {
            // Check whether the current time is in the block period
            return now < clientRequest.LastRequestTime.AddSeconds(clientRequest.BlockDuration);
        }

        private static async Task BlockClientAsync(HttpContext context)
        {
            // Set forbidden status and return a message
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync(Messages.BannedMessage);
        }

        private static bool ShouldResetRequestCount(ClientRequest clientRequest, DateTime now)
        {
            // check the reset time has passed
            return now >= clientRequest.LastRequestTime.Add(Settings.ResetTime);
        }

        private static void ResetClientRequest(ClientRequest clientRequest, DateTime now)
        {
            // Reset parameters
            clientRequest.RequestCount = 0;
            clientRequest.BlockDuration = 0;
            clientRequest.LastRequestTime = now;
        }
    }
}
