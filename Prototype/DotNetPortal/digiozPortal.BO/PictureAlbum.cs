using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class PictureAlbum
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool? Visible { get; set; }
    }
}
