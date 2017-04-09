using NSpeedTest.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSpeedTest
{
    public class SpeedTestClient : ISpeedTestClient
    {
        private const string ConfigUrl = "http://www.speedtest.net/speedtest-config.php";
        private const string ServersUrl = "http://www.speedtest.net/speedtest-servers.php";
        //private readonly int[] downloadSizes = { 350, 500, 750, 1000, 1500, 2000, 2500, 3000, 3500, 4000 };
        private readonly int[] downloadSizes = { 350, 750, 1500 };
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int MaxUploadSize = 4; // 400 KB

        #region ISpeedTestClient

        /// <summary>
        /// Download speedtest.net settings
        /// </summary>
        /// <returns>speedtest.net settings</returns>
        public async Task<Settings> GetSettings()
        {
            using (var client = new SpeedTestWebClient())
            {
                var settings = await client.GetConfig<Settings>(ConfigUrl);
                var serversConfig = await client.GetConfig<ServersList>(ServersUrl);

                serversConfig.CalculateDistances(settings.Client.GeoCoordinate);
                settings.Servers = serversConfig.Servers.OrderBy(s => s.Distance).ToList();

                return settings;
            }
        }

        /// <summary>
        /// Test latency (ping) to server
        /// </summary>
        /// <returns>Latency in milliseconds (ms)</returns>
        public async Task<int> TestServerLatency(Server server, int retryCount = 3)
        {
            var latencyUri = CreateTestUrl(server, "latency.txt");
            var timer = new Stopwatch();

            using (var client = new SpeedTestWebClient())
            {
                for (var i = 0; i < retryCount; i++)
                {
                    string testString;
                    try
                    {
                        timer.Start();
                        testString = await client.GetStringAsync(latencyUri);
                    }
                    catch
                    {
                        continue;
                    }
                    finally
                    {
                        timer.Stop();    
                    }

                    if (!testString.StartsWith("test=test"))
                    {
                        throw new InvalidOperationException("Server returned incorrect test string for latency.txt");
                    }
                }
            }

            return (int)timer.ElapsedMilliseconds / retryCount;
        }

        /// <summary>
        /// Test download speed to server
        /// </summary>
        /// <returns>Download speed in Kbps</returns>
        public double TestDownloadSpeed(Server server, int simultaneousDownloads = 2, int retryCount = 2)
        {
            var testData = GenerateDownloadUrls(server, retryCount);

            return TestSpeed(testData, async (client, url) =>
            {
                var data = await client.GetStringAsync(url).ConfigureAwait(false);
                return data.Length;
            }, simultaneousDownloads);
        }

        /// <summary>
        /// Test upload speed to server
        /// </summary>
        /// <returns>Upload speed in Kbps</returns>
        public double TestUploadSpeed(Server server, int simultaneousUploads = 2, int retryCount = 2)
        {
            var testData = GenerateUploadData(retryCount);
            return TestSpeed(testData, async (client, uploadData) =>
            {
                var data = new List<KeyValuePair<string, string>>();
                foreach (var k in uploadData.AllKeys)
                {
                    data.Add(new KeyValuePair<string, string>(k, uploadData[k]));
                }
                await client.PostAsync(server.Url, new FormUrlEncodedContent(data)).ConfigureAwait(false);
                return uploadData[0].Length;
            }, simultaneousUploads);
        }

        #endregion

        #region Helpers

        private static double TestSpeed<T>(IEnumerable<T> testData, Func<HttpClient, T, Task<int>> doWork, int concurencyCount = 2)
        {
            var timer = new Stopwatch();
            var throttler = new SemaphoreSlim(concurencyCount);

            timer.Start();
            var downloadTasks = testData.Select(async data =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                var client = new SpeedTestWebClient();
                try
                {
                    var size = await doWork(client, data).ConfigureAwait(false);
                    return size;
                }
                finally
                {
                    client.Dispose();
                    throttler.Release();
                }
            }).ToArray();

            Task.WaitAll(downloadTasks);
            timer.Stop();

            double totalSize = downloadTasks.Sum(task => task.Result);
            return (totalSize * 8 / 1024) / ((double)timer.ElapsedMilliseconds / 1000);
        }

        private static IEnumerable<NameValueCollection> GenerateUploadData(int retryCount)
        {
            var random = new Random();
            var result = new List<NameValueCollection>();

            for (var sizeCounter = 1; sizeCounter < MaxUploadSize+1; sizeCounter++)
            {
                var size = sizeCounter*200*1024;
                var builder = new StringBuilder(size);

                for (var i = 0; i < size; ++i)
                    builder.Append(Chars[random.Next(Chars.Length)]);

                for (var i = 0; i < retryCount; i++)
                {
                    result.Add(new NameValueCollection { { string.Format("content{0}", sizeCounter), builder.ToString() } });
                }
            }

            return result;
        }

        private static string CreateTestUrl(Server server, string file)
        {
            return new Uri(new Uri(server.Url), ".").OriginalString + file;
        }

        private IEnumerable<string> GenerateDownloadUrls(Server server, int retryCount)
        {
            var downloadUriBase = CreateTestUrl(server, "random{0}x{0}.jpg?r={1}");
            foreach (var downloadSize in downloadSizes)
            {
                for (var i = 0; i < retryCount; i++)
                {
                    yield return string.Format(downloadUriBase, downloadSize, i);
                }
            }
        }

        #endregion
    }
}
