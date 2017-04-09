using System;
using System.Collections.Generic;
using System.Linq;
using NSpeedTest.Models;
using System.Threading.Tasks;

namespace NSpeedTest.Client
{
    class Program
    {
        private static SpeedTestClient client;
        private static Settings settings;
        private const string DefaultCountry = "Belarus";

        static void Main()
        {
            Setup();
        }

        private static async void Setup()
        {
            Console.WriteLine("Getting speedtest.net settings and server list...");
            client = new SpeedTestClient();
            settings = await client.GetSettings();

            var servers = await SelectServers();
            var bestServer = SelectBestServer(servers);

            Console.WriteLine("Testing speed...");
            var downloadSpeed = client.TestDownloadSpeed(bestServer, settings.Download.ThreadsPerUrl);
            PrintSpeed("Download", downloadSpeed);
            var uploadSpeed = client.TestUploadSpeed(bestServer, settings.Upload.ThreadsPerUrl);
            PrintSpeed("Upload", uploadSpeed);

            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }

        private static Server SelectBestServer(IEnumerable<Server> servers)
        {
            Console.WriteLine();
            Console.WriteLine("Best server by latency:");
            var bestServer = servers.OrderBy(x => x.Latency).First();
            PrintServerDetails(bestServer);
            Console.WriteLine();
            return bestServer;
        }

        private static async Task<IEnumerable<Server>> SelectServers()
        {
            Console.WriteLine();
            Console.WriteLine("Selecting best server by distance...");
            var servers = settings.Servers.Where(s => s.Country.Equals(DefaultCountry)).Take(10).ToList();

            foreach (var server in servers)
            {
                server.Latency = await client.TestServerLatency(server);
                PrintServerDetails(server);
            }
            return servers;
        }

        private static void PrintServerDetails(Server server)
        {
            Console.WriteLine("Hosted by {0} ({1}/{2}), distance: {3}km, latency: {4}ms", server.Sponsor, server.Name,
                server.Country, (int)server.Distance / 1000, server.Latency);
        }

        private static void PrintSpeed(string type, double speed)
        {
            if (speed > 1024)
            {
                Console.WriteLine("{0} speed: {1} Mbps", type, Math.Round(speed / 1024, 2));
            }
            else
            {
                Console.WriteLine("{0} speed: {1} Kbps", type, Math.Round(speed, 2));
            }
        }
    }
}
