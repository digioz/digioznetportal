using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Poll
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Slug { get; set; }
        public bool IsClosed { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Featured { get; set; }
        public bool AllowMultipleOptionsVote { get; set; }
    }
}
