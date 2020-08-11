using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Page")]
    public partial class Page
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
