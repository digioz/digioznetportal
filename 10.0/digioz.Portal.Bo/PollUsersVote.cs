using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class PollUsersVote
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        [MaxLength(128)]
        public string PollId { get; set; }
        public string DateVoted { get; set; }
    }
}
