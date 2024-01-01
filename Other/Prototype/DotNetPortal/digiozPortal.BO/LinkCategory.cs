using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class LinkCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
