using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("CommentLike")]
    public partial class CommentLike
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CommentId { get; set; }
    }
}
