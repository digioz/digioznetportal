using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class CommentLike
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        [MaxLength(128)]
        public string CommentId { get; set; }
    }
}
