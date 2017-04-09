using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NSpeedTest
{
    internal class SpeedTestWebClient : HttpClient
    {
        public int ConnectionLimit { get; set; }

        public SpeedTestWebClient()
        {
            ConnectionLimit = 10;
        }

        public async Task<T> GetConfig<T>(string url)
        {
            var data = await GetStringAsync(url);
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        private static Uri AddTimeStamp(Uri address)
        {
            var uriBuilder = new UriBuilder(address);
            var query = QueryHelpers.ParseQuery(uriBuilder.Query);
            query["x"] = DateTime.Now.ToFileTime().ToString(CultureInfo.InvariantCulture);
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }
    }
}
