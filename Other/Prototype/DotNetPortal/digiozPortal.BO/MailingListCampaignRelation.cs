using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class MailingListCampaignRelation
    {
        public Guid Id { get; set; }
        public Guid MailingListId { get; set; }
        public Guid MailingListCampaignId { get; set; }
    }
}
