using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Menu
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Location { get; set; }
        [MaxLength(50)]
        public string Controller { get; set; }
        [MaxLength(50)]
        public string Action { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
        public int SortOrder { get; set; }
    }
}
