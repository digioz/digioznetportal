using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Picture
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int AlbumId { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public bool Approved { get; set; }
        public bool Visible { get; set; }
        public string Thumbnail { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
