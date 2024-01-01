using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Announcement> Announcements { get; set; }
        public Page Page { get; set; }
    }
}