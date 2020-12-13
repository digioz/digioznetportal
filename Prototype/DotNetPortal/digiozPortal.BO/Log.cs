using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Log
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
