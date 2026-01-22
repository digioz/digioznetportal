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
    /// Unit tests for OrderService - CRITICAL for e-commerce transactions and financial data
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Orders")]
    [Category("Critical")]
    public class OrderServiceTests
    {
        private digiozPortalContext _context;
        private OrderService _service;

        [SetUp]
        public void Setup()
        {
            _context = TestDataHelper.CreateInMemoryContext();
            _service = new OrderService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsOrder()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            var result = _service.Get("order-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("order-1");
            result.UserId.Should().Be("user-1");
            result.Total.Should().Be(99.99m);
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
        public void Get_ReturnsOrderWithAllProperties()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", total: 149.99m, trxApproved: true);
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            var result = _service.Get("order-1");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
            result.Phone.Should().Be("555-1234");
            result.BillingAddress.Should().Be("123 Test St");
            result.ShippingAddress.Should().Be("123 Test St");
            result.TrxApproved.Should().BeTrue();
            result.Total.Should().Be(149.99m);
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleOrders_ReturnsAllOrders()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1", total: 99.99m),
                TestDataHelper.CreateTestOrder("order-2", "user-2", total: 149.99m),
                TestDataHelper.CreateTestOrder("order-3", "user-3", total: 199.99m)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
            results.Sum(o => o.Total).Should().Be(449.97m);
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
        public void GetAll_IncludesApprovedAndDeclinedOrders()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1", trxApproved: true),
                TestDataHelper.CreateTestOrder("order-2", "user-2", trxApproved: false)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(o => o.TrxApproved == true);
            results.Should().Contain(o => o.TrxApproved == false);
        }

        #endregion

        #region GetByUserId Tests

        [Test]
        public void GetByUserId_WithExistingUser_ReturnsUserOrders()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1", total: 50m),
                TestDataHelper.CreateTestOrder("order-2", "user-1", total: 75m),
                TestDataHelper.CreateTestOrder("order-3", "user-2", total: 100m)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-1");

            // Assert
            results.Should().HaveCount(2);
            results.Should().OnlyContain(o => o.UserId == "user-1");
            results.Sum(o => o.Total).Should().Be(125m);
        }

        [Test]
        public void GetByUserId_WithNullUserId_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByUserId(null);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByUserId_WithEmptyUserId_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByUserId(string.Empty);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByUserId_WithNonExistingUser_ReturnsEmptyList()
        {
            // Arrange
            _context.Orders.Add(TestDataHelper.CreateTestOrder("order-1", "user-1"));
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-999");

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region CountByUserId Tests

        [Test]
        public void CountByUserId_WithExistingUser_ReturnsCorrectCount()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1"),
                TestDataHelper.CreateTestOrder("order-2", "user-1"),
                TestDataHelper.CreateTestOrder("order-3", "user-1"),
                TestDataHelper.CreateTestOrder("order-4", "user-2")
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("user-1");

            // Assert
            count.Should().Be(3);
        }

        [Test]
        public void CountByUserId_WithNullUserId_ReturnsZero()
        {
            // Act
            var count = _service.CountByUserId(null);

            // Assert
            count.Should().Be(0);
        }

        [Test]
        public void CountByUserId_WithEmptyUserId_ReturnsZero()
        {
            // Act
            var count = _service.CountByUserId(string.Empty);

            // Assert
            count.Should().Be(0);
        }

        [Test]
        public void CountByUserId_WithNonExistingUser_ReturnsZero()
        {
            // Arrange
            _context.Orders.Add(TestDataHelper.CreateTestOrder("order-1", "user-1"));
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("user-999");

            // Assert
            count.Should().Be(0);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidOrder_AddsToDatabase()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("new-order", "user-1", total: 299.99m);

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("new-order");

            saved.Should().NotBeNull();
            saved!.UserId.Should().Be("user-1");
            saved.Total.Should().Be(299.99m);
        }

        [Test]
        public void Add_WithAllProperties_SavesCorrectly()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", total: 199.99m, trxApproved: true);
            order.InvoiceNumber = "INV-001";
            order.OrderDate = new DateTime(2024, 1, 15);

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("order-1");

            saved.Should().NotBeNull();
            saved!.InvoiceNumber.Should().Be("INV-001");
            saved.OrderDate.Should().Be(new DateTime(2024, 1, 15));
            saved.FirstName.Should().Be("Test");
            saved.LastName.Should().Be("User");
            saved.Email.Should().Be("test@example.com");
            saved.TrxApproved.Should().BeTrue();
        }

        [Test]
        public void Add_WithTransactionDetails_SavesTransactionInfo()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order.TrxId = "TRX-ABC123";
            order.TrxResponseCode = "1";
            order.TrxAuthorizationCode = "AUTH789";
            order.TrxMessage = "Transaction Approved";

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("order-1");

            saved.Should().NotBeNull();
            saved!.TrxId.Should().Be("TRX-ABC123");
            saved.TrxResponseCode.Should().Be("1");
            saved.TrxAuthorizationCode.Should().Be("AUTH789");
            saved.TrxMessage.Should().Be("Transaction Approved");
        }

        [Test]
        public void Add_WithDeclinedTransaction_SavesCorrectly()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", trxApproved: false);

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("order-1");

            saved.Should().NotBeNull();
            saved!.TrxApproved.Should().BeFalse();
            saved.TrxMessage.Should().Be("Declined");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingOrder_UpdatesInDatabase()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", total: 100m);
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            order.Total = 150m;
            order.TrxApproved = false;
            _service.Update(order);

            // Assert
            var updated = _context.Orders.Find("order-1");
            updated.Total.Should().Be(150m);
            updated.TrxApproved.Should().BeFalse();
        }

        [Test]
        public void Update_ChangesShippingAddress_UpdatesCorrectly()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            order.ShippingAddress = "456 New St";
            order.ShippingCity = "New City";
            order.ShippingState = "NY";
            _service.Update(order);

            // Assert
            var updated = _context.Orders.Find("order-1");
            updated.ShippingAddress.Should().Be("456 New St");
            updated.ShippingCity.Should().Be("New City");
            updated.ShippingState.Should().Be("NY");
        }

        [Test]
        public void Update_ChangesTransactionStatus_UpdatesCorrectly()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", trxApproved: false);
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            order.TrxApproved = true;
            order.TrxAuthorizationCode = "AUTH-UPDATED";
            order.TrxMessage = "Approved on retry";
            _service.Update(order);

            // Assert
            var updated = _context.Orders.Find("order-1");
            updated.TrxApproved.Should().BeTrue();
            updated.TrxAuthorizationCode.Should().Be("AUTH-UPDATED");
            updated.TrxMessage.Should().Be("Approved on retry");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            _service.Delete("order-1");

            // Assert
            var deleted = _context.Orders.Find("order-1");
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

        #region DeleteByUserId Tests

        [Test]
        public void DeleteByUserId_WithExistingUser_RemovesAllUserOrders()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1"),
                TestDataHelper.CreateTestOrder("order-2", "user-1"),
                TestDataHelper.CreateTestOrder("order-3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var deletedCount = _service.DeleteByUserId("user-1");

            // Assert
            deletedCount.Should().Be(2);
            var remaining = _context.Orders.ToList();
            remaining.Should().HaveCount(1);
            remaining[0].UserId.Should().Be("user-2");
        }

        [Test]
        public void DeleteByUserId_WithNullUserId_ReturnsZero()
        {
            // Act
            var deletedCount = _service.DeleteByUserId(null);

            // Assert
            deletedCount.Should().Be(0);
        }

        [Test]
        public void DeleteByUserId_WithEmptyUserId_ReturnsZero()
        {
            // Act
            var deletedCount = _service.DeleteByUserId(string.Empty);

            // Assert
            deletedCount.Should().Be(0);
        }

        [Test]
        public void DeleteByUserId_WithNonExistingUser_ReturnsZero()
        {
            // Arrange
            _context.Orders.Add(TestDataHelper.CreateTestOrder("order-1", "user-1"));
            _context.SaveChanges();

            // Act
            var deletedCount = _service.DeleteByUserId("user-999");

            // Assert
            deletedCount.Should().Be(0);
        }

        [Test]
        public void DeleteByUserId_WithMultipleOrders_CalculatesTotalCorrectly()
        {
            // Arrange - User with multiple high-value orders
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1", total: 500m),
                TestDataHelper.CreateTestOrder("order-2", "user-1", total: 750m),
                TestDataHelper.CreateTestOrder("order-3", "user-1", total: 1000m)
            );
            _context.SaveChanges();

            var originalTotal = _context.Orders.Where(o => o.UserId == "user-1").Sum(o => o.Total);

            // Act
            var deletedCount = _service.DeleteByUserId("user-1");

            // Assert
            deletedCount.Should().Be(3);
            originalTotal.Should().Be(2250m); // Verify we're deleting the right total
            _context.Orders.Should().BeEmpty();
        }

        #endregion

        #region ReassignByUserId Tests

        [Test]
        public void ReassignByUserId_WithExistingUser_ReassignsAllOrders()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1"),
                TestDataHelper.CreateTestOrder("order-2", "user-1"),
                TestDataHelper.CreateTestOrder("order-3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var reassignedCount = _service.ReassignByUserId("user-1", "user-3");

            // Assert
            reassignedCount.Should().Be(2);
            var order1 = _context.Orders.Find("order-1");
            var order2 = _context.Orders.Find("order-2");
            order1.UserId.Should().Be("user-3");
            order2.UserId.Should().Be("user-3");
        }

        [Test]
        public void ReassignByUserId_WithNullFromUserId_ReturnsZero()
        {
            // Act
            var reassignedCount = _service.ReassignByUserId(null, "user-2");

            // Assert
            reassignedCount.Should().Be(0);
        }

        [Test]
        public void ReassignByUserId_WithNullToUserId_ReturnsZero()
        {
            // Act
            var reassignedCount = _service.ReassignByUserId("user-1", null);

            // Assert
            reassignedCount.Should().Be(0);
        }

        [Test]
        public void ReassignByUserId_WithEmptyUserIds_ReturnsZero()
        {
            // Act
            var reassignedCount = _service.ReassignByUserId(string.Empty, string.Empty);

            // Assert
            reassignedCount.Should().Be(0);
        }

        [Test]
        public void ReassignByUserId_WithNonExistingUser_ReturnsZero()
        {
            // Arrange
            _context.Orders.Add(TestDataHelper.CreateTestOrder("order-1", "user-1"));
            _context.SaveChanges();

            // Act
            var reassignedCount = _service.ReassignByUserId("user-999", "user-2");

            // Assert
            reassignedCount.Should().Be(0);
        }

        #endregion

        #region GetByTokenAndUserId Tests

        [Test]
        public void GetByTokenAndUserId_WithValidTokenAndUser_ReturnsOrder()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order.TrxId = "TOKEN-ABC123";
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            var result = _service.GetByTokenAndUserId("TOKEN-ABC123", "user-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("order-1");
            result.TrxId.Should().Be("TOKEN-ABC123");
        }

        [Test]
        public void GetByTokenAndUserId_WithResponseCode_FiltersCorrectly()
        {
            // Arrange
            var order1 = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order1.TrxId = "TOKEN-ABC123";
            order1.TrxResponseCode = "1";

            var order2 = TestDataHelper.CreateTestOrder("order-2", "user-1");
            order2.TrxId = "TOKEN-ABC123";
            order2.TrxResponseCode = "0";

            _context.Orders.AddRange(order1, order2);
            _context.SaveChanges();

            // Act
            var result = _service.GetByTokenAndUserId("TOKEN-ABC123", "user-1", "1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("order-1");
            result.TrxResponseCode.Should().Be("1");
        }

        [Test]
        public void GetByTokenAndUserId_WithNullToken_ReturnsNull()
        {
            // Act
            var result = _service.GetByTokenAndUserId(null, "user-1");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByTokenAndUserId_WithNullUserId_ReturnsNull()
        {
            // Act
            var result = _service.GetByTokenAndUserId("TOKEN-ABC", null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByTokenAndUserId_WithEmptyToken_ReturnsNull()
        {
            // Act
            var result = _service.GetByTokenAndUserId(string.Empty, "user-1");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByTokenAndUserId_WithWrongUser_ReturnsNull()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order.TrxId = "TOKEN-ABC123";
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            var result = _service.GetByTokenAndUserId("TOKEN-ABC123", "user-2");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByTokenAndUserId_WithWrongToken_ReturnsNull()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order.TrxId = "TOKEN-ABC123";
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            var result = _service.GetByTokenAndUserId("TOKEN-WRONG", "user-1");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByTokenAndUserId_WithWrongResponseCode_ReturnsNull()
        {
            // Arrange
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order.TrxId = "TOKEN-ABC123";
            order.TrxResponseCode = "1";
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Act
            var result = _service.GetByTokenAndUserId("TOKEN-ABC123", "user-1", "0");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Financial Data Integrity Tests

        [Test]
        public void Orders_MaintainFinancialAccuracy_AcrossOperations()
        {
            // Arrange - Create orders with specific totals
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1", total: 49.99m),
                TestDataHelper.CreateTestOrder("order-2", "user-1", total: 89.99m),
                TestDataHelper.CreateTestOrder("order-3", "user-1", total: 129.99m)
            );
            _context.SaveChanges();

            // Act - Calculate total revenue
            var orders = _service.GetByUserId("user-1");
            var totalRevenue = orders.Sum(o => o.Total);

            // Assert - Verify financial accuracy
            totalRevenue.Should().Be(269.97m);
            orders.Should().HaveCount(3);
        }

        [Test]
        public void Orders_TrackApprovedVsDeclined_Correctly()
        {
            // Arrange
            _context.Orders.AddRange(
                TestDataHelper.CreateTestOrder("order-1", "user-1", total: 100m, trxApproved: true),
                TestDataHelper.CreateTestOrder("order-2", "user-1", total: 150m, trxApproved: true),
                TestDataHelper.CreateTestOrder("order-3", "user-1", total: 200m, trxApproved: false)
            );
            _context.SaveChanges();

            // Act
            var allOrders = _service.GetByUserId("user-1");
            var approvedOrders = allOrders.Where(o => o.TrxApproved).ToList();
            var declinedOrders = allOrders.Where(o => !o.TrxApproved).ToList();

            // Assert
            approvedOrders.Sum(o => o.Total).Should().Be(250m);
            declinedOrders.Sum(o => o.Total).Should().Be(200m);
            approvedOrders.Should().HaveCount(2);
            declinedOrders.Should().HaveCount(1);
        }

        #endregion

        #region Edge Cases and Security Tests

        [Test]
        public void Orders_WithSensitiveData_StoreCorrectly()
        {
            // Arrange - Order with masked credit card
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1");
            order.Ccnumber = "XXXXXXXXXXXX1234"; // Should be masked
            order.CccardCode = "***"; // Should be masked

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("order-1");
            saved.Ccnumber.Should().Contain("XXXX"); // Verify masking
            saved.CccardCode.Should().Be("***"); // Verify masking
        }

        [Test]
        public void Orders_WithLargeTotal_HandlesCorrectly()
        {
            // Arrange - Large order amount
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", total: 9999999.99m);

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("order-1");
            saved.Total.Should().Be(9999999.99m);
        }

        [Test]
        public void Orders_WithZeroTotal_HandlesCorrectly()
        {
            // Arrange - Free order or promotional
            var order = TestDataHelper.CreateTestOrder("order-1", "user-1", total: 0m);

            // Act
            _service.Add(order);

            // Assert
            var saved = _context.Orders.Find("order-1");
            saved.Total.Should().Be(0m);
        }

        #endregion
    }
}
