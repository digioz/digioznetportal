using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingListSubscriberRelation
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string MailingListId { get; set; }
        [MaxLength(128)]
        public string MailingListSubscriberId { get; set; }
    }
}
