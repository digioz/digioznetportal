using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingListCampaign
    {
        public string Id { get; set; }
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
