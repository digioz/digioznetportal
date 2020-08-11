using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Video")]
    public partial class Video
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long? AlbumId { get; set; }
        public string Filename { get; set; }
        public string Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool? Approved { get; set; }
        public bool? Visible { get; set; }
        public string Thumbnail { get; set; }
    }
}
