using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Models.ViewModels
{
    public class CommentPostViewModel
    {
        public string Id { get; set; }
        [DisplayName("Parent")]
        public string ParentId { get; set; }
        [DisplayName("User")]
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Body { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
        [DisplayName("Modified Date")]
        public DateTime? ModifiedDate { get; set; }
        public int Likes { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
    }
}
