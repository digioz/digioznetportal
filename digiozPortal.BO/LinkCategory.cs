using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("LinkCategory")]
    public partial class LinkCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
