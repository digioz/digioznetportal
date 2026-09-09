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
    public class ListModel : PageModel
    {
        private readonly IShoppingCartService _cartService;
        private readonly IProductService _productService;

        public ListModel(
            IShoppingCartService cartService,
            IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        public class CartItemViewModel
        {
            public string? Id { get; set; }
            public string? ProductId { get; set; }
            public string? ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal CartTotal { get; set; }

        public void OnGet()
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (string.IsNullOrEmpty(userId))
                return;

            // Get all cart items for current user
            var cartItems = _cartService.GetAll()
                .Where(c => c.UserId == userId)
                .ToList();

            CartTotal = 0;

            foreach (var item in cartItems)
            {
                var product = _productService.Get(item.ProductId);
                if (product != null)
                {
                    CartItems.Add(new CartItemViewModel
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });

                    CartTotal += product.Price * item.Quantity;
                }
            }
        }
    }
}