using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class MailingList
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DefaultEmailFrom { get; set; }
        public string DefaultFromName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
    }
}
