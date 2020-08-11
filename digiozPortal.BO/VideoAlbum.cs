﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("VideoAlbum")]
    public partial class VideoAlbum
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool? Visible { get; set; }
    }
}
