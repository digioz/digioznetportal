using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Vote")]
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
