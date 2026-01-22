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
    /// Unit tests for ProductService - CRITICAL for product catalog, inventory, and e-commerce
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Products")]
    [Category("Critical")]
    public class ProductServiceTests
    {
        private digiozPortalContext _context;
        private ProductService _service;

        [SetUp]
        public void Setup()
        {
            _context = TestDataHelper.CreateInMemoryContext();
            _service = new ProductService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsProduct()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Widget", 49.99m);
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var result = _service.Get("prod-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("prod-1");
            result.Name.Should().Be("Widget");
            result.Price.Should().Be(49.99m);
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
        public void Get_ReturnsProductWithAllProperties()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Premium Widget", 149.99m, "cat-1");
            product.Make = "ACME";
            product.Model = "X1000";
            product.UnitsInStock = 50;

            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var result = _service.Get("prod-1");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Premium Widget");
            result.Price.Should().Be(149.99m);
            result.ProductCategoryId.Should().Be("cat-1");
            result.Make.Should().Be("ACME");
            result.Model.Should().Be("X1000");
            result.UnitsInStock.Should().Be(50);
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleProducts_ReturnsAllProducts()
        {
            // Arrange
            _context.Products.AddRange(
                TestDataHelper.CreateTestProduct("prod-1", "Product 1", 29.99m),
                TestDataHelper.CreateTestProduct("prod-2", "Product 2", 49.99m),
                TestDataHelper.CreateTestProduct("prod-3", "Product 3", 99.99m)
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
        public void GetAll_IncludesVisibleAndHiddenProducts()
        {
            // Arrange
            _context.Products.AddRange(
                TestDataHelper.CreateTestProduct("prod-1", "Visible Product", 29.99m, visible: true),
                TestDataHelper.CreateTestProduct("prod-2", "Hidden Product", 49.99m, visible: false)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(p => p.Visible);
            results.Should().Contain(p => !p.Visible);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidProduct_AddsToDatabase()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("new-prod", "New Product", 79.99m);

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("new-prod");

            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Product");
            saved.Price.Should().Be(79.99m);
        }

        [Test]
        public void Add_WithAllProperties_SavesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Complete Product", 199.99m, "cat-1");
            product.Make = "TestMake";
            product.Model = "TestModel";
            product.Sku = "SKU-12345";
            product.Cost = 120m;
            product.UnitsInStock = 75;
            product.OutOfStock = false;
            product.Weight = "2.5 lbs";
            product.Dimensions = "12x10x3 inches";
            product.PartNumber = "PN-ABC123";
            product.ManufacturerUrl = "https://example.com";

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-1");

            saved.Should().NotBeNull();
            saved!.Make.Should().Be("TestMake");
            saved.Model.Should().Be("TestModel");
            saved.Sku.Should().Be("SKU-12345");
            saved.Cost.Should().Be(120m);
            saved.UnitsInStock.Should().Be(75);
            saved.OutOfStock.Should().BeFalse();
            saved.Weight.Should().Be("2.5 lbs");
            saved.Dimensions.Should().Be("12x10x3 inches");
        }

        [Test]
        public void Add_WithCategoryId_AssociatesWithCategory()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Categorized Product", 49.99m, "electronics-1");

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-1");
            saved.ProductCategoryId.Should().Be("electronics-1");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingProduct_UpdatesInDatabase()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Original Name", 50m);
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            product.Name = "Updated Name";
            product.Price = 75m;
            _service.Update(product);

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Name.Should().Be("Updated Name");
            updated.Price.Should().Be(75m);
        }

        [Test]
        public void Update_ChangesInventory_UpdatesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.UnitsInStock = 100;
            product.OutOfStock = false;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Product sold out
            product.UnitsInStock = 0;
            product.OutOfStock = true;
            _service.Update(product);

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.UnitsInStock.Should().Be(0);
            updated.OutOfStock.Should().BeTrue();
        }

        [Test]
        public void Update_ChangesVisibility_UpdatesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m, visible: true);
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Hide product
            product.Visible = false;
            _service.Update(product);

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Visible.Should().BeFalse();
        }

        [Test]
        public void Update_ChangesCategory_UpdatesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m, "cat-1");
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Move to different category
            product.ProductCategoryId = "cat-2";
            _service.Update(product);

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.ProductCategoryId.Should().Be("cat-2");
        }

        [Test]
        public void Update_ChangesPriceAndCost_UpdatesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 100m);
            product.Cost = 60m;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Price increase
            product.Price = 120m;
            product.Cost = 70m;
            _service.Update(product);

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Price.Should().Be(120m);
            updated.Cost.Should().Be(70m);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            _service.Delete("prod-1");

            // Assert
            var deleted = _context.Products.Find("prod-1");
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

        #region IncrementViews Tests

        [Test]
        public void IncrementViews_WithExistingProduct_IncrementsViewCount()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.Views = 10;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            _service.IncrementViews("prod-1");

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Views.Should().Be(11);
        }

        [Test]
        public void IncrementViews_FromZero_IncrementsToOne()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.Views = 0;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            _service.IncrementViews("prod-1");

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Views.Should().Be(1);
        }

        [Test]
        public void IncrementViews_MultipleIncrements_AccumulatesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.Views = 0;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Simulate multiple product views
            _service.IncrementViews("prod-1");
            _service.IncrementViews("prod-1");
            _service.IncrementViews("prod-1");

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Views.Should().Be(3);
        }

        [Test]
        public void IncrementViews_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.IncrementViews("nonexistent");
            act.Should().NotThrow();
        }

        #endregion

        #region ClearProductCategory Tests

        [Test]
        public void ClearProductCategory_WithExistingCategory_ClearsAllProducts()
        {
            // Arrange
            _context.Products.AddRange(
                TestDataHelper.CreateTestProduct("prod-1", "Product 1", 29.99m, "cat-1"),
                TestDataHelper.CreateTestProduct("prod-2", "Product 2", 49.99m, "cat-1"),
                TestDataHelper.CreateTestProduct("prod-3", "Product 3", 99.99m, "cat-2")
            );
            _context.SaveChanges();

            // Act
            _service.ClearProductCategory("cat-1");

            // Assert
            var prod1 = _context.Products.Find("prod-1");
            var prod2 = _context.Products.Find("prod-2");
            var prod3 = _context.Products.Find("prod-3");

            prod1.ProductCategoryId.Should().BeNull();
            prod2.ProductCategoryId.Should().BeNull();
            prod3.ProductCategoryId.Should().Be("cat-2"); // Unchanged
        }

        [Test]
        public void ClearProductCategory_WithNonExistingCategory_DoesNothing()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m, "cat-1");
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            _service.ClearProductCategory("cat-999");

            // Assert
            var unchanged = _context.Products.Find("prod-1");
            unchanged.ProductCategoryId.Should().Be("cat-1");
        }

        [Test]
        public void ClearProductCategory_WithNullCategory_HandlesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m, "cat-1");
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act & Assert
            Action act = () => _service.ClearProductCategory(null);
            act.Should().NotThrow();

            var unchanged = _context.Products.Find("prod-1");
            unchanged.ProductCategoryId.Should().Be("cat-1");
        }

        #endregion

        #region Inventory Management Tests

        [Test]
        public void Product_InventoryDepletion_TracksCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.UnitsInStock = 100;
            product.OutOfStock = false;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Simulate sales to 50 units
            _context.ChangeTracker.Clear();
            var toUpdate1 = _service.Get("prod-1");
            toUpdate1.UnitsInStock = 50;
            _service.Update(toUpdate1);
            
            _context.ChangeTracker.Clear();
            var after50 = _service.Get("prod-1");

            // Simulate sales to 0 units
            _context.ChangeTracker.Clear();
            var toUpdate2 = _service.Get("prod-1");
            toUpdate2.UnitsInStock = 0;
            toUpdate2.OutOfStock = true;
            _service.Update(toUpdate2);
            
            _context.ChangeTracker.Clear();
            var after0 = _service.Get("prod-1");

            // Assert
            after50.UnitsInStock.Should().Be(50);
            after50.OutOfStock.Should().BeFalse();
            
            after0.UnitsInStock.Should().Be(0);
            after0.OutOfStock.Should().BeTrue();
        }

        [Test]
        public void Product_Restocking_UpdatesInventory()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.UnitsInStock = 0;
            product.OutOfStock = true;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Restock
            product.UnitsInStock = 100;
            product.OutOfStock = false;
            _service.Update(product);

            // Assert
            var restocked = _context.Products.Find("prod-1");
            restocked.UnitsInStock.Should().Be(100);
            restocked.OutOfStock.Should().BeFalse();
        }

        #endregion

        #region Pricing Tests

        [Test]
        public void Product_ProfitMargin_CanBeCalculated()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 100m);
            product.Cost = 60m;
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var saved = _context.Products.Find("prod-1");
            var profit = saved.Price - saved.Cost.Value;
            var margin = (profit / saved.Price) * 100;

            // Assert
            profit.Should().Be(40m);
            margin.Should().Be(40m); // 40% margin
        }

        [Test]
        public void Product_WithNoCost_HandlesNullCost()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 100m);
            product.Cost = null;

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-1");
            saved.Cost.Should().BeNull();
        }

        [Test]
        public void Product_PriceIncrease_UpdatesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 100m);
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - 10% price increase
            product.Price = 110m;
            _service.Update(product);

            // Assert
            var updated = _context.Products.Find("prod-1");
            updated.Price.Should().Be(110m);
        }

        #endregion

        #region Visibility and Catalog Tests

        [Test]
        public void Product_VisibilityToggle_WorksCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m, visible: true);
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act - Hide product
            _context.ChangeTracker.Clear();
            var toUpdate1 = _service.Get("prod-1");
            toUpdate1.Visible = false;
            _service.Update(toUpdate1);
            
            _context.ChangeTracker.Clear();
            var afterHidden = _service.Get("prod-1");

            // Show product again
            _context.ChangeTracker.Clear();
            var toUpdate2 = _service.Get("prod-1");
            toUpdate2.Visible = true;
            _service.Update(toUpdate2);
            
            _context.ChangeTracker.Clear();
            var afterShown = _service.Get("prod-1");

            // Assert
            afterHidden.Visible.Should().BeFalse();
            afterShown.Visible.Should().BeTrue();
        }

        [Test]
        public void Product_FilterByCategory_WorksCorrectly()
        {
            // Arrange
            _context.Products.AddRange(
                TestDataHelper.CreateTestProduct("prod-1", "Electronics 1", 99.99m, "electronics"),
                TestDataHelper.CreateTestProduct("prod-2", "Electronics 2", 149.99m, "electronics"),
                TestDataHelper.CreateTestProduct("prod-3", "Clothing 1", 29.99m, "clothing")
            );
            _context.SaveChanges();

            // Act
            var electronics = _service.GetAll().Where(p => p.ProductCategoryId == "electronics").ToList();

            // Assert
            electronics.Should().HaveCount(2);
            electronics.Should().OnlyContain(p => p.ProductCategoryId == "electronics");
        }

        #endregion

        #region Real-World Scenario Tests

        [Test]
        public void Product_CompleteLifecycle_WorksCorrectly()
        {
            // Arrange - New product added
            var product = TestDataHelper.CreateTestProduct("prod-1", "New Product", 99.99m, "cat-1");
            product.UnitsInStock = 100;
            product.Visible = false; // Hidden until ready
            product.Views = 0;

            // Act & Assert - Step 1: Add product
            _service.Add(product);
            var created = _service.Get("prod-1");
            created.Should().NotBeNull();
            created!.Visible.Should().BeFalse();

            // Step 2: Make visible
            created.Visible = true;
            _service.Update(created);

            // Step 3: Product viewed multiple times
            _service.IncrementViews("prod-1");
            _service.IncrementViews("prod-1");
            _service.IncrementViews("prod-1");

            // Step 4: Inventory depletes
            created.UnitsInStock = 10;
            _service.Update(created);

            // Step 5: Price increase
            created.Price = 109.99m;
            _service.Update(created);

            // Final verification
            var final = _service.Get("prod-1");
            final.Visible.Should().BeTrue();
            final.Views.Should().Be(3);
            final.UnitsInStock.Should().Be(10);
            final.Price.Should().Be(109.99m);
        }

        [Test]
        public void Product_BulkCategoryChange_WorksCorrectly()
        {
            // Arrange - Products in old category
            _context.Products.AddRange(
                TestDataHelper.CreateTestProduct("prod-1", "Product 1", 29.99m, "old-cat"),
                TestDataHelper.CreateTestProduct("prod-2", "Product 2", 49.99m, "old-cat"),
                TestDataHelper.CreateTestProduct("prod-3", "Product 3", 99.99m, "other-cat")
            );
            _context.SaveChanges();

            // Act - Clear old category (simulate category deletion)
            _service.ClearProductCategory("old-cat");

            // Assert
            var products = _service.GetAll();
            products.Where(p => p.Id.StartsWith("prod-1") || p.Id.StartsWith("prod-2"))
                .Should().OnlyContain(p => p.ProductCategoryId == null);
            products.First(p => p.Id == "prod-3").ProductCategoryId.Should().Be("other-cat");
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void Product_WithZeroPrice_SavesCorrectly()
        {
            // Arrange - Free product
            var product = TestDataHelper.CreateTestProduct("prod-1", "Free Product", 0m);

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-1");
            saved.Price.Should().Be(0m);
        }

        [Test]
        public void Product_WithHighPrice_HandlesLargeAmounts()
        {
            // Arrange - Expensive product
            var product = TestDataHelper.CreateTestProduct("prod-luxury", "Luxury Item", 99999.99m);

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-luxury");
            saved.Price.Should().Be(99999.99m);
        }

        [Test]
        public void Product_WithLongDescription_SavesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product", 49.99m);
            product.Description = new string('A', 1000);

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-1");
            saved.Description.Should().HaveLength(1000);
        }

        [Test]
        public void Product_WithSpecialCharacters_SavesCorrectly()
        {
            // Arrange
            var product = TestDataHelper.CreateTestProduct("prod-1", "Product™ & Co. <Special>", 49.99m);

            // Act
            _service.Add(product);

            // Assert
            var saved = _context.Products.Find("prod-1");
            saved.Name.Should().Be("Product™ & Co. <Special>");
        }

        #endregion
    }
}
