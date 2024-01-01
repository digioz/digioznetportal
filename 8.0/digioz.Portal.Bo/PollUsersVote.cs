using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class PollUsersVote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PollId { get; set; }
        public string DateVoted { get; set; }
    }
}
