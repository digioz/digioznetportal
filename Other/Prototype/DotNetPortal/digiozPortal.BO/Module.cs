using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Module
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Location { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool DisplayInBox { get; set; }
    }
}
