using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Chat
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
