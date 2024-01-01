using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace digiozPortal.BO
{
    [Table("CommentConfig")]
    public partial class CommentConfig
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceTitle { get; set; }
        public bool Visible { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
