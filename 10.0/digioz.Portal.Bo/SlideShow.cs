using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class SlideShow
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string Image { get; set; }
        [MaxLength(128)]
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
