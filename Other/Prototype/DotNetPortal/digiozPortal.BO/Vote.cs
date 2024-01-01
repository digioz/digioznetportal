using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Vote
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public DateTime? DateVoted { get; set; }
        public string VotedByMembershipUserId { get; set; }
        public Guid PostId { get; set; }
        public string MembershipUserId { get; set; }
    }
}
