using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class CommentConfig
    {
        [MaxLength(128)]
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceTitle { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
