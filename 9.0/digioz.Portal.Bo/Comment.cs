using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Comment
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string ParentId { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public string Username { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string Body { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int Likes { get; set; }
        public bool? Visible { get; set; }
        public bool? Approved { get; set; }
    }
}
