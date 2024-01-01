using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Post
    {
        public Guid Id { get; set; }
        public string PostContent { get; set; }
        public DateTime DateCreated { get; set; }
        public int VoteCount { get; set; }
        public DateTime DateEdited { get; set; }
        public bool IsSolution { get; set; }
        public bool IsTopicStarter { get; set; }
        public bool? FlaggedAsSpam { get; set; }
        public string IpAddress { get; set; }
        public Guid TopicId { get; set; }
        public string MembershipUserId { get; set; }
    }
}
