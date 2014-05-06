using System;
using System.Device.Location;
using System.Xml.Serialization;

namespace NSpeedTest.Models
{
    [XmlRoot("client")]
    public class Client
    {
        [XmlAttribute("ip")]
        public string Ip { get; set; }

        [XmlAttribute("lat")]
        public double Latitude { get; set; }

        [XmlAttribute("lon")]
        public double Longitude { get; set; }

        [XmlAttribute("isp")]
        public string Isp { get; set; }

        [XmlAttribute("isprating")]
        public double IspRating { get; set; }

        [XmlAttribute("rating")]
        public double Rating { get; set; }

        [XmlAttribute("ispdlavg")]
        public int IspAvarageDownloadSpeed { get; set; }

        [XmlAttribute("ispulavg")]
        public int IspAvarageUploadSpeed { get; set; }

        private Lazy<GeoCoordinate> geoCoordinate;

        public GeoCoordinate GeoCoordinate
        {
            get { return geoCoordinate.Value; }
        }

        public Client()
        {
            // note: geo coordinate will not be recalculated on Latitude or Longitude change
            geoCoordinate = new Lazy<GeoCoordinate>(() => new GeoCoordinate(Latitude, Longitude));
        }
    }
}