using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Web;
using System.Xml.Serialization;

namespace NSpeedTest
{
    internal class SpeedTestWebClient : WebClient
    {
        public int ConnectionLimit { get; set; }

        public SpeedTestWebClient()
        {
            ConnectionLimit = 10;
        }

        public T GetConfig<T>(string url)
        {
            var data = DownloadString(url);
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(AddTimeStamp(address)) as HttpWebRequest;
            
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            request.ServicePoint.ConnectionLimit = ConnectionLimit;

            return request;
        }

        private static Uri AddTimeStamp(Uri address)
        {
            var uriBuilder = new UriBuilder(address);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["x"] = DateTime.Now.ToFileTime().ToString(CultureInfo.InvariantCulture);
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }
    }
}
