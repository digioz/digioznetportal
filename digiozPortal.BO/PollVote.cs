using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class PollVote
    {
        public Guid Id { get; set; }
        public Guid PollAnswerId { get; set; }
        public string MembershipUserId { get; set; }
    }
}
