using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class VisitorInfo
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string PageUrl { get; set; }
        public string ReferringUrl { get; set; }
        public string BrowserName { get; set; }
        public string BrowserType { get; set; }
        public string BrowserUserAgent { get; set; }
        public string BrowserVersion { get; set; }
        public bool IsCrawler { get; set; }
        public string JsVersion { get; set; }
        public string OperatingSystem { get; set; }
        public string Keywords { get; set; }
        public string SearchEngine { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
