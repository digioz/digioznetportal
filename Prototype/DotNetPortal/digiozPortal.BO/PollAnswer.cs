using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class PollAnswer
    {
        public Guid Id { get; set; }
        public string Answer { get; set; }
        public Guid PollId { get; set; }
    }
}
