using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for ShoppingCartService - E-commerce shopping cart management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("ECommerce")]
    public class ShoppingCartServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ShoppingCartService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ShoppingCartService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsShoppingCart()
        {
            // Arrange
            var cart = new ShoppingCart
            {
                Id = "cart-1",
                UserId = "user-123",
                ProductId = "prod-456",
                Quantity = 2,
                DateCreated = DateTime.UtcNow,
                Size = "Medium",
                Color = "Blue"
            };
            _context.ShoppingCarts.Add(cart);
            _context.SaveChanges();

            // Act
            var result = _service.Get("cart-1");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-123");
            result.Quantity.Should().Be(2);
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleCartItems_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.ShoppingCarts.AddRange(
                new ShoppingCart { Id = "cart-1", UserId = "user-1", ProductId = "prod-1", Quantity = 1, DateCreated = now, Color = "Red" },
                new ShoppingCart { Id = "cart-2", UserId = "user-1", ProductId = "prod-2", Quantity = 2, DateCreated = now, Color = "Blue" },
                new ShoppingCart { Id = "cart-3", UserId = "user-2", ProductId = "prod-1", Quantity = 1, DateCreated = now, Color = "Green" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoCartItems_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidCartItem_AddsToDatabase()
        {
            // Arrange
            var cart = new ShoppingCart
            {
                Id = "new-cart",
                UserId = "new-user",
                ProductId = "new-prod",
                Quantity = 3,
                DateCreated = DateTime.UtcNow,
                Size = "Large",
                Color = "Black",
                MaterialType = "Cotton"
            };

            // Act
            _service.Add(cart);

            // Assert
            var saved = _context.ShoppingCarts.Find("new-cart");
            saved.Should().NotBeNull();
            saved!.Quantity.Should().Be(3);
            saved.MaterialType.Should().Be("Cotton");
        }

        [Test]
        public void Add_MultipleCartItems_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var carts = new[]
            {
                new ShoppingCart { Id = "c-1", UserId = "user", ProductId = "p-1", Quantity = 1, DateCreated = now, Color = "Red" },
                new ShoppingCart { Id = "c-2", UserId = "user", ProductId = "p-2", Quantity = 2, DateCreated = now, Color = "Blue" },
                new ShoppingCart { Id = "c-3", UserId = "user", ProductId = "p-3", Quantity = 1, DateCreated = now, Color = "Green" }
            };

            // Act
            foreach (var cart in carts)
            {
                _service.Add(cart);
            }

            // Assert
            _context.ShoppingCarts.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingCart_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new ShoppingCart
            {
                Id = "cart-update",
                UserId = "user-1",
                ProductId = "prod-1",
                Quantity = 1,
                DateCreated = now,
                Color = "Red"
            };
            _context.ShoppingCarts.Add(cart);
            _context.SaveChanges();

            // Act
            cart.Quantity = 5;
            cart.Color = "Blue";
            _service.Update(cart);

            // Assert
            var updated = _context.ShoppingCarts.Find("cart-update");
            updated!.Quantity.Should().Be(5);
            updated.Color.Should().Be("Blue");
        }

        [Test]
        public void Update_ChangeQuantity_Updates()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new ShoppingCart
            {
                Id = "qty-cart",
                UserId = "user",
                ProductId = "prod",
                Quantity = 1,
                DateCreated = now
            };
            _context.ShoppingCarts.Add(cart);
            _context.SaveChanges();

            // Act
            cart.Quantity = 10;
            _service.Update(cart);

            // Assert
            var updated = _context.ShoppingCarts.Find("qty-cart");
            updated!.Quantity.Should().Be(10);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new ShoppingCart
            {
                Id = "delete-cart",
                UserId = "user",
                ProductId = "prod",
                Quantity = 1,
                DateCreated = now
            };
            _context.ShoppingCarts.Add(cart);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-cart");

            // Assert
            var deleted = _context.ShoppingCarts.Find("delete-cart");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent");
            act.Should().NotThrow();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void ShoppingCart_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new ShoppingCart
            {
                Id = "lifecycle-cart",
                UserId = "lifecycle-user",
                ProductId = "lifecycle-prod",
                Quantity = 1,
                DateCreated = now,
                Color = "Red",
                Size = "Medium"
            };

            // Act - Add
            _service.Add(cart);
            var added = _service.Get("lifecycle-cart");
            added.Should().NotBeNull();

            // Act - Update
            added!.Quantity = 3;
            added.Color = "Blue";
            _service.Update(added);
            var updated = _service.Get("lifecycle-cart");
            updated!.Quantity.Should().Be(3);
            updated.Color.Should().Be("Blue");

            // Act - Delete
            _service.Delete("lifecycle-cart");
            var deleted = _service.Get("lifecycle-cart");
            deleted.Should().BeNull();
        }

        [Test]
        public void ShoppingCart_UserCart_WithMultipleItems()
        {
            // Arrange - Simulate a user building a shopping cart
            var now = DateTime.UtcNow;
            var userId = "shopper-1";

            var cartItems = new[]
            {
                new ShoppingCart { Id = "item-1", UserId = userId, ProductId = "shirt-1", Quantity = 2, DateCreated = now, Color = "Blue", Size = "M" },
                new ShoppingCart { Id = "item-2", UserId = userId, ProductId = "pants-1", Quantity = 1, DateCreated = now, Color = "Black", Size = "L" },
                new ShoppingCart { Id = "item-3", UserId = userId, ProductId = "shoes-1", Quantity = 1, DateCreated = now, Color = "White", Size = "10" }
            };

            // Act
            foreach (var item in cartItems)
            {
                _service.Add(item);
            }

            var allItems = _service.GetAll();
            var userCartItems = allItems.Where(c => c.UserId == userId).ToList();
            var totalQuantity = userCartItems.Sum(c => c.Quantity);

            // Assert
            allItems.Should().HaveCount(3);
            userCartItems.Should().HaveCount(3);
            totalQuantity.Should().Be(4);
        }

        [Test]
        public void ShoppingCart_UpdateQuantityAndVariants()
        {
            // Arrange - Simulate updating cart with different sizes/colors
            var now = DateTime.UtcNow;
            var cart = new ShoppingCart
            {
                Id = "variant-cart",
                UserId = "user",
                ProductId = "product",
                Quantity = 1,
                DateCreated = now,
                Size = "Small",
                Color = "Red",
                MaterialType = "Polyester",
                Notes = "Gift wrap please"
            };
            _service.Add(cart);

            // Act - Update quantity and notes
            var updated = _service.Get("variant-cart");
            updated!.Quantity = 5;
            updated.Notes = "Rush delivery";
            _service.Update(updated);

            // Assert
            var result = _service.Get("variant-cart");
            result!.Quantity.Should().Be(5);
            result.Notes.Should().Be("Rush delivery");
            result.Size.Should().Be("Small");
            result.Color.Should().Be("Red");
        }

        #endregion
    }
}
