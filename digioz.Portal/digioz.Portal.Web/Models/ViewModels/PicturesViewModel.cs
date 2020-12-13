using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Models.ViewModels
{
    public class PicturesViewModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> Timestamp { get; set; }
        public Nullable<bool> Visible { get; set; }
        public string Thumbnail { get; set; }
    }
}
