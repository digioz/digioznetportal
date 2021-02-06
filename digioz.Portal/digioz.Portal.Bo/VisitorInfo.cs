using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class VisitorInfo
    {
        public long Id { get; set; }
        public bool? JavaEnabled { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public int? ScreenHeight { get; set; }
        public int? ScreenWidth { get; set; }
        public string BrowserEngineName { get; set; }
        public string Host { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Referrer { get; set; }
        public string Href { get; set; }
        public string UserAgent { get; set; }
        public string UserLanguage { get; set; }
        public string SessionId { get; set; }
    }
}
