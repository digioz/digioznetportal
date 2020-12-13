using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Picture
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long? AlbumId { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool? Approved { get; set; }
        public bool? Visible { get; set; }
    }
}
