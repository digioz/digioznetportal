using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Announcement
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
