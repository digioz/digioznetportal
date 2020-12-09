using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class LogVisitor
    {
        public int Id { get; set; }
        public string Ipaddress { get; set; }
        public string BrowserType { get; set; }
        public string Language { get; set; }
        public bool IsBot { get; set; }
        public string Country { get; set; }
        public string ReferringUrl { get; set; }
        public string SearchString { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
