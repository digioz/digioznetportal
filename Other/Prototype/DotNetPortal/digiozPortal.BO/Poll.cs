using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Poll
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public bool IsClosed { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Featured { get; set; }
        public bool AllowMultipleOptionsVote { get; set; }
        public string MembershipUserId { get; set; }
    }
}
