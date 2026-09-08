using System;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        private readonly IProductService _productService;

        public CartController(
            IShoppingCartService cartService,
            IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        /// <summary>
        /// Add item to shopping cart
        /// </summary>
        [HttpPost("add")]
        public IActionResult Add([FromBody] AddToCartRequest? request)
        {
            if (string.IsNullOrEmpty(request?.ProductId))
            {
                return BadRequest(new { success = false, message = "Product ID is required" });
            }

            var product = _productService.Get(request.ProductId);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Not authenticated" });
            }

            try
            {
                // Check if item already exists in cart
                var existingItem = _cartService.GetAll()
                    .FirstOrDefault(c => c.UserId == userId && c.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    // Update quantity
                    existingItem.Quantity += request.Quantity;
                    _cartService.Update(existingItem);
                }
                else
                {
                    // Add new item
                    var cartItem = new ShoppingCart
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        DateCreated = DateTime.Now
                    };
                    _cartService.Add(cartItem);
                }

                return Ok(new { success = true, message = "Item added to cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Remove item from shopping cart
        /// </summary>
        [HttpDelete("remove/{cartId}")]
        public IActionResult Remove(string cartId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var cartItem = _cartService.Get(cartId);
                if (cartItem == null || cartItem.UserId != userId)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }

                _cartService.Delete(cartId);
                return Ok(new { success = true, message = "Item removed from cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("update")]
        public IActionResult Update([FromBody] UpdateCartRequest? request)
        {
            if (string.IsNullOrEmpty(request?.Id))
            {
                return BadRequest(new { success = false, message = "Cart item ID is required" });
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var cartItem = _cartService.Get(request.Id);
                if (cartItem == null || cartItem.UserId != userId)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }

                if (request.Quantity < 1)
                {
                    return BadRequest(new { success = false, message = "Quantity must be at least 1" });
                }

                cartItem.Quantity = request.Quantity;
                _cartService.Update(cartItem);

                return Ok(new { success = true, message = "Cart updated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Clear shopping cart
        /// </summary>
        [HttpDelete("empty")]
        public IActionResult ClearCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Not authenticated" });
            }

            try
            {
                var cartItems = _cartService.GetAll()
                    .Where(c => c.UserId == userId)
                    .ToList();

                foreach (var item in cartItems)
                {
                    _cartService.Delete(item.Id);
                }

                return Ok(new { success = true, message = "Cart emptied" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private string? GetUserId()
        {
            return User.FindFirst("sub")?.Value 
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        }
    }

    public class AddToCartRequest
    {
        public string? ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartRequest
    {
        public string? Id { get; set; }
        public int Quantity { get; set; }
    }
}
