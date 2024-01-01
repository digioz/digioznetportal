using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingListSubscriberRelation
    {
        public string Id { get; set; }
        public string MailingListId { get; set; }
        public string MailingListSubscriberId { get; set; }
    }
}
