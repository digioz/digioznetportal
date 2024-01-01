using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("PictureAlbum")]
    public partial class PictureAlbum
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool? Visible { get; set; }
    }
}
