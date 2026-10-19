using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Zone
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [StringLength(50)]
        public string Location { get; set; }
        public string Body { get; set; }
        public bool Visible { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
