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
    /// Unit tests for ProductCategoryService - Product category management for e-commerce
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("ECommerce")]
    public class ProductCategoryServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ProductCategoryService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ProductCategoryService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsProductCategory()
        {
            // Arrange
            var category = new ProductCategory
            {
                Id = "electronics",
                Name = "Electronics",
                Description = "Electronic products and gadgets",
                Visible = true,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _context.ProductCategories.Add(category);
            _context.SaveChanges();

            // Act
            var result = _service.Get("electronics");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Electronics");
            result.Visible.Should().BeTrue();
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
        public void GetAll_WithMultipleCategories_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.ProductCategories.AddRange(
                new ProductCategory { Id = "cat1", Name = "Category 1", Description = "Desc1", Visible = true, DateCreated = now, DateModified = now },
                new ProductCategory { Id = "cat2", Name = "Category 2", Description = "Desc2", Visible = false, DateCreated = now, DateModified = now },
                new ProductCategory { Id = "cat3", Name = "Category 3", Description = "Desc3", Visible = true, DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoCategories_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidCategory_AddsToDatabase()
        {
            // Arrange
            var category = new ProductCategory
            {
                Id = "clothing",
                Name = "Clothing",
                Description = "Apparel and accessories",
                Visible = true,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            // Act
            _service.Add(category);

            // Assert
            var saved = _context.ProductCategories.Find("clothing");
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Clothing");
        }

        [Test]
        public void Add_WithInvisibleCategory_Saves()
        {
            // Arrange
            var category = new ProductCategory
            {
                Id = "draft",
                Name = "Draft Category",
                Description = "Not yet published",
                Visible = false,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            // Act
            _service.Add(category);

            // Assert
            var saved = _context.ProductCategories.Find("draft");
            saved!.Visible.Should().BeFalse();
        }

        [Test]
        public void Add_MultipleCategories_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var categories = new[]
            {
                new ProductCategory { Id = "c1", Name = "C1", Description = "D1", Visible = true, DateCreated = now, DateModified = now },
                new ProductCategory { Id = "c2", Name = "C2", Description = "D2", Visible = true, DateCreated = now, DateModified = now },
                new ProductCategory { Id = "c3", Name = "C3", Description = "D3", Visible = false, DateCreated = now, DateModified = now }
            };

            // Act
            foreach (var cat in categories)
            {
                _service.Add(cat);
            }

            // Assert
            _context.ProductCategories.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingCategory_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var category = new ProductCategory
            {
                Id = "original",
                Name = "Original Name",
                Description = "Original description",
                Visible = false,
                DateCreated = now,
                DateModified = now
            };
            _context.ProductCategories.Add(category);
            _context.SaveChanges();

            // Act
            category.Name = "Updated Name";
            category.Description = "Updated description";
            category.Visible = true;
            _service.Update(category);

            // Assert
            var updated = _context.ProductCategories.Find("original");
            updated!.Name.Should().Be("Updated Name");
            updated.Description.Should().Be("Updated description");
            updated.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_ToggleVisibility_Updates()
        {
            // Arrange
            var category = new ProductCategory
            {
                Id = "toggle",
                Name = "Toggle Category",
                Description = "Test",
                Visible = true,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _context.ProductCategories.Add(category);
            _context.SaveChanges();

            // Act
            category.Visible = false;
            _service.Update(category);

            // Assert
            var updated = _context.ProductCategories.Find("toggle");
            updated!.Visible.Should().BeFalse();
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var category = new ProductCategory
            {
                Id = "to-delete",
                Name = "Delete Me",
                Description = "Temporary",
                Visible = true,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _context.ProductCategories.Add(category);
            _context.SaveChanges();

            // Act
            _service.Delete("to-delete");

            // Assert
            var deleted = _context.ProductCategories.Find("to-delete");
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
        public void ProductCategory_FullLifecycle()
        {
            // Arrange
            var category = new ProductCategory
            {
                Id = "lifecycle",
                Name = "Lifecycle Category",
                Description = "Initial description",
                Visible = false,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            // Act - Add
            _service.Add(category);
            var added = _service.Get("lifecycle");
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Category";
            added.Visible = true;
            _service.Update(added);
            var updated = _service.Get("lifecycle");
            updated!.Name.Should().Be("Updated Lifecycle Category");
            updated.Visible.Should().BeTrue();

            // Act - Delete
            _service.Delete("lifecycle");
            var deleted = _service.Get("lifecycle");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageProductCatalog_WithVisibilityControl()
        {
            // Arrange - Create categories for a product store
            var now = DateTime.UtcNow;
            _service.Add(new ProductCategory { Id = "electronics", Name = "Electronics", Description = "Gadgets", Visible = true, DateCreated = now, DateModified = now });
            _service.Add(new ProductCategory { Id = "books", Name = "Books", Description = "Literature", Visible = true, DateCreated = now, DateModified = now });
            _service.Add(new ProductCategory { Id = "upcoming", Name = "Upcoming Products", Description = "Coming soon", Visible = false, DateCreated = now, DateModified = now });

            // Act
            var allCategories = _service.GetAll();
            var visibleCategories = allCategories.Where(c => c.Visible).ToList();
            var hiddenCategories = allCategories.Where(c => !c.Visible).ToList();

            // Assert
            allCategories.Should().HaveCount(3);
            visibleCategories.Should().HaveCount(2);
            hiddenCategories.Should().HaveCount(1);
        }

        #endregion
    }
}
