using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Bo.ViewModels
{
    public class StoreMenuViewModel
    {
        public List<ProductCategory> ProductCategories { get; set; }
        public bool IsEnabled { get; set; }
    }
}
