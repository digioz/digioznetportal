using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class PollUsersVote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PollId { get; set; }
        public DateTime DateVoted { get; set; }
    }
}
