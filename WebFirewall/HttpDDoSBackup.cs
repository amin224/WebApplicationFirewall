using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebFirewall
{
    public class HttpDDoSProtection2
    {
        private static ConcurrentDictionary<string, (int requestCount, DateTime lastRequestTime, int blockDuration)> _clientRequestData = new ConcurrentDictionary<string, (int, DateTime, int)>();
        private static readonly int _maxRequestsPerMinute = 10;
        private static readonly TimeSpan _resetTime = TimeSpan.FromMinutes(1);
        private static readonly int _initialBlockDurationSeconds = 60;
        private static readonly int _extendedBlockDurationSeconds = 300;

        public HttpDDoSProtection()
        {

        }

        public async Task InvokeAsync(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress.ToString();
            var now = DateTime.UtcNow;

            if (_clientRequestData.TryGetValue(clientIp, out var clientData))
            {
                var (requestCount, lastRequestTime, blockDuration) = clientData;

                // Check client is currently blocked?
                if (blockDuration > 0 && now < lastRequestTime.AddSeconds(blockDuration))
                {
                    context.Response.StatusCode = blockDuration == _initialBlockDurationSeconds ? StatusCodes.Status429TooManyRequests : StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Client is blocked. Don't try again :)");
                    return;
                }

                // Reset request count if a blocked duration has passed since the last request
                if (now >= lastRequestTime.Add(_resetTime))
                {
                    requestCount = 0;
                    blockDuration = 0;
                }

                requestCount++;

                if (requestCount > _maxRequestsPerMinute)
                {
                    blockDuration = blockDuration == 0 ? _initialBlockDurationSeconds : _extendedBlockDurationSeconds;
                    _clientRequestData[clientIp] = (requestCount, now, blockDuration);
                    context.Response.StatusCode = blockDuration == _initialBlockDurationSeconds ? StatusCodes.Status429TooManyRequests : StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Client is blocked. Don't try again :)");
                    return;
                }

                _clientRequestData[clientIp] = (requestCount, now, blockDuration);
            }
            else
            {
                _clientRequestData[clientIp] = (1, now, 0);
            }

            await _next(context);
        }
    }
}
