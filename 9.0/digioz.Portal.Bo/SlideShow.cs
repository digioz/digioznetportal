using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class SlideShow
    {
        public string Id { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
