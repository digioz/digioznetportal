using digioz.Portal.Bo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Models.ViewModels
{
    public class OrderManagerViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
