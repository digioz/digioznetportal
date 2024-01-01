using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("MailingListCampaignRelation")]
    public partial class MailingListCampaignRelation
    {
        public Guid Id { get; set; }
        public Guid MailingListId { get; set; }
        public Guid MailingListCampaignId { get; set; }
    }
}
