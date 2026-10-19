using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Store
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private const int PAGE_SIZE = 10;

        public HistoryModel(
            IOrderService orderService,
            IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        public List<Order> Orders { get; set; } = new();
        public Dictionary<string, List<OrderDetail>> OrderDetailMap { get; set; } = new();
        public int pageNumber { get; set; } = 1;
        public int pageSize => PAGE_SIZE;
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PAGE_SIZE);

        public void OnGet(int pageNumber = 1)
        {
            this.pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (string.IsNullOrEmpty(userId))
                return;

            // Get all orders for current user
            var allOrders = _orderService.GetByUserId(userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            TotalCount = allOrders.Count;

            // Apply pagination
            Orders = allOrders
                .Skip((this.pageNumber - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            // Get order details for each order
            foreach (var order in Orders)
            {
                var details = _orderDetailService.GetAll()
                    .Where(d => d.OrderId == order.Id)
                    .ToList();
                
                OrderDetailMap[order.Id] = details;
            }
        }
    }
}