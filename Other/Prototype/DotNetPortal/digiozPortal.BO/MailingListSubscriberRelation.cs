using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class MailingListSubscriberRelation
    {
        public Guid Id { get; set; }
        public Guid MailingListId { get; set; }
        public Guid MailingListSubscriberId { get; set; }
    }
}
