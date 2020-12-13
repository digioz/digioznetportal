using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digiozPortal.BO;

namespace digiozPortal.Web.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Announcement> Announcements { get; set; }
        public Page Page { get; set; }
    }
}