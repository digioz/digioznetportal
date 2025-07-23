using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class MailingList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DefaultEmailFrom { get; set; }
        public string DefaultFromName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
    }
}
