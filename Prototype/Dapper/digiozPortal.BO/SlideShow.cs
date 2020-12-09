using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("SlideShow")]
    public partial class SlideShow
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}
