using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Link")]
    public partial class Link
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public string URL { get; set; }
        public string Description { get; set; }
        public int LinkCategoryId { get; set; }
        public bool? Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
