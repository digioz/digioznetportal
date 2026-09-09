using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Link
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Url]
        public string Url { get; set; }
        public string Description { get; set; }
        public int LinkCategory { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
        public int Views { get; set; }
        public bool? Approved { get; set; }
    }
}
