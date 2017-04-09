﻿using NSpeedTest.Models;
using System.Threading.Tasks;

namespace NSpeedTest
{
    public interface ISpeedTestClient
    {
        /// <summary>
        /// Download speedtest.net settings
        /// </summary>
        /// <returns>speedtest.net settings</returns>
        Task<Settings> GetSettings();

        /// <summary>
        /// Test latency (ping) to server
        /// </summary>
        /// <returns>Latency in milliseconds (ms)</returns>
        Task<int> TestServerLatency(Server server, int retryCount = 3);

        /// <summary>
        /// Test download speed to server
        /// </summary>
        /// <returns>Download speed in Kbps</returns>
        double TestDownloadSpeed(Server server, int simultaniousDownloads = 2, int retryCount = 2);

        /// <summary>
        /// Test upload speed to server
        /// </summary>
        /// <returns>Upload speed in Kbps</returns>
        double TestUploadSpeed(Server server, int simultaniousUploads = 2, int retryCount = 2);
    }
}