using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class LogVisitor
    {
        public long Id { get; set; }
        public string Ipaddress { get; set; }
        public string BrowserType { get; set; }
        public string Language { get; set; }
        public bool? IsBot { get; set; }
        public string Country { get; set; }
        public string ReferringUrl { get; set; }
        public string SearchString { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
