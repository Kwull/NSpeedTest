using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace NSpeedTest.Tests
{
    [TestFixture]
    public class NSpeedTestApiTests
    {
        private ISpeedTestClient speedTestClientClient;

        [TestFixtureSetUp]
        public void Setup()
        {
            speedTestClientClient = new SpeedTestClient();
        }

        [Test]
        public void Should_return_settings_with_sorted_server_list_by_distance()
        {
            var settings = speedTestClientClient.GetSettings();

            for (var i = 1; i < settings.Servers.Count; i++)
            {
                Assert.IsTrue(settings.Servers[i - 1].Distance.CompareTo(settings.Servers[i].Distance) <= 0);
            }
        }

        [Test, Ignore]
        public void Should_return_settings_with_filtered_server_list_by_ignored_ids()
        {
            var settings = speedTestClientClient.GetSettings();

            var ignoredIds = settings.ServerConfig.IgnoreIds.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

            var servers = settings.Servers.Where(s => ignoredIds.Contains(s.Id.ToString(CultureInfo.InvariantCulture)));
            Assert.IsEmpty(servers);
        }

        [Test]
        public void Should_test_latency_to_server()
        {
            var settings = speedTestClientClient.GetSettings();
            var latency = speedTestClientClient.TestServerLatency(settings.Servers.First());
            Console.WriteLine("Latency: {0} ms", latency);

            Assert.Greater(latency, 0);
            Assert.Less(latency, 1000*60*5);
        }

        [Test]
        public void Should_test_download_speed()
        {
            var settings = speedTestClientClient.GetSettings();
            var speed = speedTestClientClient.TestDownloadSpeed(settings.Servers.First(), settings.Download.ThreadsPerUrl);

            PrintSpeed("Download", speed);
            Assert.Greater(speed, 0);
        }

        [Test]
        public void Should_test_upload_speed()
        {
            var settings = speedTestClientClient.GetSettings();
            var speed = speedTestClientClient.TestUploadSpeed(settings.Servers.First(), settings.Upload.ThreadsPerUrl);

            PrintSpeed("Upload", speed);
            Assert.Greater(speed, 0);
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
