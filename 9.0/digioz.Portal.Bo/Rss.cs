using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Rss
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; }
        public string Url { get; set; }
        public int MaxCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
