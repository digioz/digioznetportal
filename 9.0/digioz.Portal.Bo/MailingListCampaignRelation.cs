using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingListCampaignRelation
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string MailingListId { get; set; }
        [MaxLength(128)]
        public string MailingListCampaignId { get; set; }
    }
}
