using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("MailingListSubscriberRelation")]
    public partial class MailingListSubscriberRelation
    {
        public Guid Id { get; set; }
        public Guid MailingListId { get; set; }
        public Guid MailingListSubscriberId { get; set; }
    }
}
