using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("PollAnswer")]
    public partial class PollAnswer
    {
        public Guid Id { get; set; }
        public string Answer { get; set; }
        public Guid PollId { get; set; }
    }
}
