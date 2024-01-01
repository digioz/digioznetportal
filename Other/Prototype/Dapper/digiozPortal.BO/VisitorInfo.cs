using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("VisitorInfo")]
    public partial class VisitorInfo
    {
        public int Id { get; set; }
        public string Ipaddress { get; set; }
        public string PageUrl { get; set; }
        public string ReferringUrl { get; set; }
        public string BrowserName { get; set; }
        public string BrowserType { get; set; }
        public string BrowserUserAgent { get; set; }
        public string BrowserVersion { get; set; }
        public bool IsCrawler { get; set; }
        public string Jsversion { get; set; }
        public string OperatingSystem { get; set; }
        public string Keywords { get; set; }
        public string SearchEngine { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
