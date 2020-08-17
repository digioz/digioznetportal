using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace digiozPortal.BO
{
    [Table("CommentLike")]
    public partial class CommentLike
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CommentId { get; set; }
    }
}
