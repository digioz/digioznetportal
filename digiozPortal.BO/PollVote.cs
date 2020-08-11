using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("PollVote")]
    public partial class PollVote
    {
        public Guid Id { get; set; }
        public Guid PollAnswerId { get; set; }
        public string MembershipUserId { get; set; }
    }
}
