using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class CommentConfig
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceTitle { get; set; }
        public bool Visible { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
