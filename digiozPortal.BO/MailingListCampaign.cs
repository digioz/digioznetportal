using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("MailingListCampaign")]
    public partial class MailingListCampaign
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Summary { get; set; }
        public string Banner { get; set; }
        public string Body { get; set; }
        public int VisitorCount { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
