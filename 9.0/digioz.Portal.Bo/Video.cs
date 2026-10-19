using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Video
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public int AlbumId { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public bool Approved { get; set; }
        public bool Visible { get; set; }
        public string Thumbnail { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Views { get; set; }
    }
}
