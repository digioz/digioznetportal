using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("MailingList")]
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
