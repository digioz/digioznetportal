using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace digioz.Portal.Web.Models.ViewModels
{
    public class CommentsMenuViewModel
    {
        public bool CommentsEnabled { get; set; }
        public int ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public int Count { get; set; }
        public int Likes { get; set; }
    }
}