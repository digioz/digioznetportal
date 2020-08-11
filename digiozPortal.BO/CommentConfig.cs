using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("CommentConfig")]
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
