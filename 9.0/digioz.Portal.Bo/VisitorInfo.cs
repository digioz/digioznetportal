using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class VisitorInfo
    {
        public long Id { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Host { get; set; }
        public string HostName { get; set; }
        [MaxLength(25)]
        public string IpAddress { get; set; }
        [MaxLength(25)]
        public string Platform { get; set; }
        public string Referrer { get; set; }
        public string Href { get; set; }
        public string UserAgent { get; set; }
        [MaxLength(25)]
        public string UserLanguage { get; set; }
        [MaxLength(25)]
        public string SessionId { get; set; }
	}
}
