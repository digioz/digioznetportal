using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using digioz.Portal.Tests.Helpers;
using System;
using System.Linq;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for OrderDetailService - CRITICAL for order line items and fulfillment accuracy
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Orders")]
    [Category("Critical")]
    public class OrderDetailServiceTests
    {
        private digiozPortalContext _context;
        private OrderDetailService _service;

        [SetUp]
        public void Setup()
        {
            _context = TestDataHelper.CreateInMemoryContext();
            _service = new OrderDetailService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsOrderDetail()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act
            var result = _service.Get("detail-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("detail-1");
            result.OrderId.Should().Be("order-1");
            result.ProductId.Should().Be("product-1");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_ReturnsOrderDetailWithAllProperties()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 3, unitPrice: 49.99m);
            detail.Description = "Premium Test Product";
            detail.Size = "Large";
            detail.Color = "Red";
            detail.MaterialType = "Polyester";
            detail.Notes = "Gift wrap requested";

            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act
            var result = _service.Get("detail-1");

            // Assert
            result.Should().NotBeNull();
            result!.Quantity.Should().Be(3);
            result.UnitPrice.Should().Be(49.99m);
            result.Description.Should().Be("Premium Test Product");
            result.Size.Should().Be("Large");
            result.Color.Should().Be("Red");
            result.MaterialType.Should().Be("Polyester");
            result.Notes.Should().Be("Gift wrap requested");
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleOrderDetails_ReturnsAllDetails()
        {
            // Arrange
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 2, unitPrice: 50m),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 1, unitPrice: 100m),
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-2", "product-3", quantity: 3, unitPrice: 25m)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetAll_CanCalculateTotalForOrder()
        {
            // Arrange - Order with multiple line items
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 2, unitPrice: 50m),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 1, unitPrice: 100m),
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-1", "product-3", quantity: 3, unitPrice: 25m)
            );
            _context.SaveChanges();

            // Act
            var orderDetails = _service.GetAll().Where(d => d.OrderId == "order-1").ToList();
            var orderTotal = orderDetails.Sum(d => d.Quantity * d.UnitPrice);

            // Assert
            orderDetails.Should().HaveCount(3);
            orderTotal.Should().Be(275m); // (2*50) + (1*100) + (3*25) = 100 + 100 + 75
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidOrderDetail_AddsToDatabase()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("new-detail", "order-1", "product-1", quantity: 5, unitPrice: 29.99m);

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("new-detail");

            saved.Should().NotBeNull();
            saved!.OrderId.Should().Be("order-1");
            saved.ProductId.Should().Be("product-1");
            saved.Quantity.Should().Be(5);
            saved.UnitPrice.Should().Be(29.99m);
        }

        [Test]
        public void Add_WithAllProperties_SavesCorrectly()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 2, unitPrice: 149.99m);
            detail.Description = "Premium Widget XL";
            detail.Size = "XL";
            detail.Color = "Black";
            detail.MaterialType = "Aluminum";
            detail.Notes = "Express shipping";

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");

            saved.Should().NotBeNull();
            saved!.Description.Should().Be("Premium Widget XL");
            saved.Size.Should().Be("XL");
            saved.Color.Should().Be("Black");
            saved.MaterialType.Should().Be("Aluminum");
            saved.Notes.Should().Be("Express shipping");
        }

        [Test]
        public void Add_MultipleLineItemsForSameOrder_SavesCorrectly()
        {
            // Arrange - Multiple items in one order
            var detail1 = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 1, unitPrice: 99.99m);
            var detail2 = TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 2, unitPrice: 49.99m);
            var detail3 = TestDataHelper.CreateTestOrderDetail("detail-3", "order-1", "product-3", quantity: 1, unitPrice: 29.99m);

            // Act
            _service.Add(detail1);
            _service.Add(detail2);
            _service.Add(detail3);

            // Assert
            var orderDetails = _context.OrderDetails.Where(d => d.OrderId == "order-1").ToList();
            orderDetails.Should().HaveCount(3);
            var total = orderDetails.Sum(d => d.Quantity * d.UnitPrice);
            total.Should().Be(229.96m); // 99.99 + (2*49.99) + 29.99
        }

        [Test]
        public void Add_WithZeroQuantity_SavesCorrectly()
        {
            // Arrange - Edge case: cancelled item but keeping record
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 0, unitPrice: 99.99m);

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");

            saved.Should().NotBeNull();
            saved!.Quantity.Should().Be(0);
        }

        [Test]
        public void Add_WithHighQuantity_SavesCorrectly()
        {
            // Arrange - Bulk order
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 1000, unitPrice: 5.99m);

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");

            saved.Should().NotBeNull();
            saved!.Quantity.Should().Be(1000);
            var lineTotal = saved != null ? saved.Quantity * saved.UnitPrice : 0m;
            lineTotal.Should().Be(5990m);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingOrderDetail_UpdatesInDatabase()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 1, unitPrice: 50m);
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act - Update quantity and price
            detail.Quantity = 3;
            detail.UnitPrice = 45m;
            _service.Update(detail);

            // Assert
            var updated = _context.OrderDetails.Find("detail-1");
            updated!.Quantity.Should().Be(3);
            updated!.UnitPrice.Should().Be(45m);
        }

        [Test]
        public void Update_ChangesProductOptions_UpdatesCorrectly()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act - Customer changes size and color
            detail.Size = "XL";
            detail.Color = "Green";
            detail.Notes = "Changed from Medium Blue";
            _service.Update(detail);

            // Assert
            var updated = _context.OrderDetails.Find("detail-1");
            updated!.Size.Should().Be("XL");
            updated!.Color.Should().Be("Green");
            updated!.Notes.Should().Be("Changed from Medium Blue");
        }

        [Test]
        public void Update_ChangesDescription_UpdatesCorrectly()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act - Update product description
            detail.Description = "Updated Product Description";
            _service.Update(detail);

            // Assert
            var updated = _context.OrderDetails.Find("detail-1");
            updated!.Description.Should().Be("Updated Product Description");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act
            _service.Delete("detail-1");

            // Assert
            var deleted = _context.OrderDetails.Find("detail-1");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent");
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_OneLineItem_DoesNotAffectOtherLineItems()
        {
            // Arrange - Multiple line items
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1"),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2"),
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-1", "product-3")
            );
            _context.SaveChanges();

            // Act - Delete one line item
            _service.Delete("detail-2");

            // Assert
            var remaining = _context.OrderDetails.Where(d => d.OrderId == "order-1").ToList();
            remaining.Should().HaveCount(2);
            remaining.Should().NotContain(d => d.Id == "detail-2");
            remaining.Should().Contain(d => d.Id == "detail-1");
            remaining.Should().Contain(d => d.Id == "detail-3");
        }

        #endregion

        #region Order Total Calculation Tests

        [Test]
        public void OrderDetails_CalculateCorrectLineTotal_ForSingleItem()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 3, unitPrice: 25.50m);
            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            // Act
            var saved = _context.OrderDetails.Find("detail-1");
            var lineTotal = saved != null ? saved.Quantity * saved.UnitPrice : 0m;

            // Assert
            lineTotal.Should().Be(76.50m); // 3 * 25.50
        }

        [Test]
        public void OrderDetails_CalculateCorrectOrderTotal_ForMultipleItems()
        {
            // Arrange - Typical order with multiple products
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 2, unitPrice: 19.99m),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 1, unitPrice: 49.99m),
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-1", "product-3", quantity: 3, unitPrice: 9.99m)
            );
            _context.SaveChanges();

            // Act
            var orderDetails = _context.OrderDetails.Where(d => d.OrderId == "order-1").ToList();
            var orderTotal = orderDetails.Sum(d => d.Quantity * d.UnitPrice);

            // Assert
            orderTotal.Should().Be(119.94m); // (2*19.99) + 49.99 + (3*9.99) = 39.98 + 49.99 + 29.97
        }

        [Test]
        public void OrderDetails_HandleDecimalPrecision_Correctly()
        {
            // Arrange - Products with precise pricing
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 7, unitPrice: 3.33m),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 3, unitPrice: 12.99m)
            );
            _context.SaveChanges();

            // Act
            var orderDetails = _context.OrderDetails.Where(d => d.OrderId == "order-1").ToList();
            var orderTotal = orderDetails.Sum(d => d.Quantity * d.UnitPrice);

            // Assert
            orderTotal.Should().Be(62.28m); // (7*3.33) + (3*12.99) = 23.31 + 38.97
        }

        #endregion

        #region Multiple Order Tests

        [Test]
        public void OrderDetails_SeparateItemsByOrder_Correctly()
        {
            // Arrange - Multiple orders with their own line items
            _context.OrderDetails.AddRange(
                // Order 1 items
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 2, unitPrice: 50m),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 1, unitPrice: 100m),
                // Order 2 items
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-2", "product-1", quantity: 1, unitPrice: 50m),
                TestDataHelper.CreateTestOrderDetail("detail-4", "order-2", "product-3", quantity: 3, unitPrice: 25m)
            );
            _context.SaveChanges();

            // Act
            var order1Details = _context.OrderDetails.Where(d => d.OrderId == "order-1").ToList();
            var order2Details = _context.OrderDetails.Where(d => d.OrderId == "order-2").ToList();

            // Assert
            order1Details.Should().HaveCount(2);
            order2Details.Should().HaveCount(2);
            
            var order1Total = order1Details.Sum(d => d.Quantity * d.UnitPrice);
            var order2Total = order2Details.Sum(d => d.Quantity * d.UnitPrice);
            
            order1Total.Should().Be(200m); // (2*50) + 100
            order2Total.Should().Be(125m); // 50 + (3*25)
        }

        [Test]
        public void OrderDetails_GetByOrderId_ReturnsOnlyOrderItems()
        {
            // Arrange
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1"),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2"),
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-2", "product-1")
            );
            _context.SaveChanges();

            // Act
            var order1Details = _service.GetAll().Where(d => d.OrderId == "order-1").ToList();

            // Assert
            order1Details.Should().HaveCount(2);
            order1Details.Should().OnlyContain(d => d.OrderId == "order-1");
        }

        #endregion

        #region Product Variants Tests

        [Test]
        public void OrderDetails_SameProduct_DifferentVariants_TrackedSeparately()
        {
            // Arrange - Same product, different sizes and colors
            var detail1 = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 1, unitPrice: 49.99m);
            detail1.Size = "Small";
            detail1.Color = "Red";
            
            var detail2 = TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-1", quantity: 2, unitPrice: 49.99m);
            detail2.Size = "Medium";
            detail2.Color = "Blue";
            
            var detail3 = TestDataHelper.CreateTestOrderDetail("detail-3", "order-1", "product-1", quantity: 1, unitPrice: 49.99m);
            detail3.Size = "Large";
            detail3.Color = "Red";
            
            _context.OrderDetails.AddRange(detail1, detail2, detail3);
            _context.SaveChanges();

            // Act
            var orderDetails = _service.GetAll().Where(d => d.OrderId == "order-1").ToList();
            var totalQuantity = orderDetails.Sum(d => d.Quantity);
            var totalAmount = orderDetails.Sum(d => d.Quantity * d.UnitPrice);

            // Assert
            orderDetails.Should().HaveCount(3);
            orderDetails.Should().OnlyContain(d => d.ProductId == "product-1");
            totalQuantity.Should().Be(4); // 1 + 2 + 1
            totalAmount.Should().Be(199.96m); // 4 * 49.99
        }

        [Test]
        public void OrderDetails_WithMaterialType_StoresCorrectly()
        {
            // Arrange - Products with different materials
            var detail1 = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 1, unitPrice: 100m);
            detail1.MaterialType = "Leather";
            
            var detail2 = TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 1, unitPrice: 150m);
            detail2.MaterialType = "Canvas";
            
            _context.OrderDetails.AddRange(detail1, detail2);
            _context.SaveChanges();

            // Act
            var saved = _service.GetAll().Where(d => d.OrderId == "order-1").ToList();

            // Assert
            saved.Should().Contain(d => d.MaterialType == "Leather");
            saved.Should().Contain(d => d.MaterialType == "Canvas");
        }

        #endregion

        #region Special Instructions Tests

        [Test]
        public void OrderDetails_WithNotes_StoresCustomerInstructions()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            detail.Notes = "Please gift wrap with blue ribbon";

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");
            saved.Notes.Should().Be("Please gift wrap with blue ribbon");
        }

        [Test]
        public void OrderDetails_WithEmptyNotes_SavesCorrectly()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            detail.Notes = "";

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");
            saved.Notes.Should().BeEmpty();
        }

        [Test]
        public void OrderDetails_WithLongDescription_SavesCorrectly()
        {
            // Arrange
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1");
            detail.Description = "Premium Quality Extra Long Product Description with Multiple Features and Specifications";

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");
            saved.Description.Should().Be("Premium Quality Extra Long Product Description with Multiple Features and Specifications");
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void OrderDetails_WithLargeQuantity_CalculatesCorrectly()
        {
            // Arrange - Bulk wholesale order
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 10000, unitPrice: 0.99m);

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");
            saved.Quantity.Should().Be(10000);
            var lineTotal = saved != null ? saved.Quantity * saved.UnitPrice : 0m;
            lineTotal.Should().Be(9900m);
        }

        [Test]
        public void OrderDetails_WithSmallUnitPrice_MaintainsPrecision()
        {
            // Arrange - Low-cost items
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 10, unitPrice: 0.05m);

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");
            saved.UnitPrice.Should().Be(0.05m);
            var lineTotal = saved != null ? saved.Quantity * saved.UnitPrice : 0m;
            lineTotal.Should().Be(0.50m);
        }

        [Test]
        public void OrderDetails_WithHighPriceItem_HandlesLargeAmounts()
        {
            // Arrange - Expensive item
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-luxury", quantity: 1, unitPrice: 999999.99m);

            // Act
            _service.Add(detail);

            // Assert
            var saved = _context.OrderDetails.Find("detail-1");
            saved.UnitPrice.Should().Be(999999.99m);
        }

        #endregion

        #region Fulfillment Scenario Tests

        [Test]
        public void OrderDetails_ForFulfillment_ContainsAllNecessaryInfo()
        {
            // Arrange - Complete line item for shipping
            var detail = TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 2, unitPrice: 79.99m);
            detail.Description = "Premium Bluetooth Speaker";
            detail.Size = "Standard";
            detail.Color = "Black";
            detail.MaterialType = "Plastic/Metal";
            detail.Notes = "Fragile - Handle with care";

            // Act
            _service.Add(detail);

            // Assert - Verify all fulfillment info present
            var saved = _context.OrderDetails.Find("detail-1");

            saved.Should().NotBeNull();
            saved!.ProductId.Should().NotBeNullOrEmpty();
            saved.Quantity.Should().BeGreaterThan(0);
            saved.Description.Should().NotBeNullOrEmpty();
            saved.Size.Should().NotBeNullOrEmpty();
            saved.Color.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void OrderDetails_MultipleItems_CanBePrioritized()
        {
            // Arrange - Order with various items
            _context.OrderDetails.AddRange(
                TestDataHelper.CreateTestOrderDetail("detail-1", "order-1", "product-1", quantity: 1, unitPrice: 500m),
                TestDataHelper.CreateTestOrderDetail("detail-2", "order-1", "product-2", quantity: 5, unitPrice: 10m),
                TestDataHelper.CreateTestOrderDetail("detail-3", "order-1", "product-3", quantity: 2, unitPrice: 100m)
            );
            _context.SaveChanges();

            // Act - Sort by line total for fulfillment priority
            var orderDetails = _service.GetAll()
                .Where(d => d.OrderId == "order-1")
                .OrderByDescending(d => d.Quantity * d.UnitPrice)
                .ToList();

            // Assert - High-value items first
            orderDetails[0].ProductId.Should().Be("product-1"); // $500
            orderDetails[1].ProductId.Should().Be("product-3"); // $200
            orderDetails[2].ProductId.Should().Be("product-2"); // $50
        }

        #endregion
    }
}
