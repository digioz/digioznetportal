using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace digioz.Portal.Web.Models.ViewModels
{
    public class SearchPagesViewModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string Body { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
    }
}