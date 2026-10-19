using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Announcement
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
