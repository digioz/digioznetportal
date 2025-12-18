using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Store
{
    public class OrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        public OrderConfirmationModel(
            IOrderService orderService,
            IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        public Order? Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new();

        public void OnGet(string orderId)
        {
            Order = _orderService.Get(orderId);
            
            if (Order != null)
            {
                OrderDetails = _orderDetailService.GetAll()
                    .Where(d => d.OrderId == orderId)
                    .ToList();
            }
        }
    }
}
