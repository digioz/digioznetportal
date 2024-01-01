using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Rss
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int MaxCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
