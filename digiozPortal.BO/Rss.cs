using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Rss")]
    public partial class Rss
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int MaxCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
