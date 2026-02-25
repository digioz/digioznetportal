using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Module
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        [MaxLength(255)]
        public string Location { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string Body { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool DisplayInBox { get; set; }
    }
}
