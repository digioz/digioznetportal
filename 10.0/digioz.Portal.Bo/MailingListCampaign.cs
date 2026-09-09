using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingListCampaign
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string Subject { get; set; }
        [Required]
        [MaxLength(50)]
        public string FromName { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string FromEmail { get; set; }
        [Required]
        [MaxLength(255)]
        public string Summary { get; set; }
        [MaxLength(255)]
        public string Banner { get; set; }
        [Required]
        public string Body { get; set; }
        public int VisitorCount { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
