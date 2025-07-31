using System;
using System.Collections.Generic;
using System.Linq;

namespace digioz.Portal.Bo.ViewModels
{
    public class SearchPagesViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Body { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
    }
}