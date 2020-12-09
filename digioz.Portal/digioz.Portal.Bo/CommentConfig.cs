using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class CommentConfig
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
