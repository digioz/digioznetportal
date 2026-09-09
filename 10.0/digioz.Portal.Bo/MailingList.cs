using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingList
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string DefaultEmailFrom { get; set; }
        [Required]
        public string DefaultFromName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
