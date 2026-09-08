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
    /// Unit tests for ProductOptionService - Product options/variants management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("ECommerce")]
    public class ProductOptionServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ProductOptionService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ProductOptionService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsProductOption()
        {
            // Arrange
            var option = new ProductOption
            {
                Id = "opt-1",
                ProductId = "prod-1",
                OptionType = "Color",
                OptionValue = "Red"
            };
            _context.ProductOptions.Add(option);
            _context.SaveChanges();

            // Act
            var result = _service.Get("opt-1");

            // Assert
            result.Should().NotBeNull();
            result!.OptionType.Should().Be("Color");
            result.OptionValue.Should().Be("Red");
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
        public void GetAll_WithMultipleOptions_ReturnsAll()
        {
            // Arrange
            _context.ProductOptions.AddRange(
                new ProductOption { Id = "opt-1", ProductId = "prod-1", OptionType = "Color", OptionValue = "Red" },
                new ProductOption { Id = "opt-2", ProductId = "prod-1", OptionType = "Size", OptionValue = "Large" },
                new ProductOption { Id = "opt-3", ProductId = "prod-2", OptionType = "Color", OptionValue = "Blue" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoOptions_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidOption_AddsToDatabase()
        {
            // Arrange
            var option = new ProductOption
            {
                Id = "opt-new",
                ProductId = "prod-new",
                OptionType = "Material",
                OptionValue = "Leather"
            };

            // Act
            _service.Add(option);

            // Assert
            var saved = _context.ProductOptions.Find("opt-new");
            saved.Should().NotBeNull();
            saved!.OptionType.Should().Be("Material");
        }

        [Test]
        public void Add_MultipleOptionsSameProduct_Saves()
        {
            // Arrange
            var options = new[]
            {
                new ProductOption { Id = "s-1", ProductId = "shirt", OptionType = "Size", OptionValue = "S" },
                new ProductOption { Id = "s-2", ProductId = "shirt", OptionType = "Size", OptionValue = "M" },
                new ProductOption { Id = "s-3", ProductId = "shirt", OptionType = "Size", OptionValue = "L" }
            };

            // Act
            foreach (var option in options)
            {
                _service.Add(option);
            }

            // Assert
            _context.ProductOptions.Should().HaveCount(3);
            var sizeOptions = _context.ProductOptions
                .Where(o => o.ProductId == "shirt" && o.OptionType == "Size")
                .ToList();
            sizeOptions.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingOption_UpdatesInDatabase()
        {
            // Arrange
            var option = new ProductOption
            {
                Id = "opt-1",
                ProductId = "prod-1",
                OptionType = "Original Type",
                OptionValue = "Original Value"
            };
            _context.ProductOptions.Add(option);
            _context.SaveChanges();

            // Act
            option.OptionType = "Updated Type";
            option.OptionValue = "Updated Value";
            _service.Update(option);

            // Assert
            var updated = _context.ProductOptions.Find("opt-1");
            updated!.OptionType.Should().Be("Updated Type");
            updated.OptionValue.Should().Be("Updated Value");
        }

        [Test]
        public void Update_ChangeOptionValue_Updates()
        {
            // Arrange
            var option = new ProductOption
            {
                Id = "color-opt",
                ProductId = "shirt",
                OptionType = "Color",
                OptionValue = "Blue"
            };
            _context.ProductOptions.Add(option);
            _context.SaveChanges();

            // Act
            option.OptionValue = "Green";
            _service.Update(option);

            // Assert
            var updated = _context.ProductOptions.Find("color-opt");
            updated!.OptionValue.Should().Be("Green");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var option = new ProductOption
            {
                Id = "delete-me",
                ProductId = "prod-temp",
                OptionType = "Temp",
                OptionValue = "Delete"
            };
            _context.ProductOptions.Add(option);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-me");

            // Assert
            var deleted = _context.ProductOptions.Find("delete-me");
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
        public void ProductOption_FullLifecycle()
        {
            // Arrange
            var option = new ProductOption
            {
                Id = "lifecycle-opt",
                ProductId = "lifecycle-prod",
                OptionType = "Initial Type",
                OptionValue = "Initial Value"
            };

            // Act - Add
            _service.Add(option);
            var added = _service.Get("lifecycle-opt");
            added.Should().NotBeNull();

            // Act - Update
            added!.OptionType = "Updated Type";
            added.OptionValue = "Updated Value";
            _service.Update(added);
            var updated = _service.Get("lifecycle-opt");
            updated!.OptionType.Should().Be("Updated Type");

            // Act - Delete
            _service.Delete("lifecycle-opt");
            var deleted = _service.Get("lifecycle-opt");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageProductVariants_WithMultipleOptions()
        {
            // Arrange - Create options for a product with multiple variants
            var productOptions = new[]
            {
                new ProductOption { Id = "jacket-black-s", ProductId = "winter-jacket", OptionType = "Color", OptionValue = "Black" },
                new ProductOption { Id = "jacket-black-m", ProductId = "winter-jacket", OptionType = "Color", OptionValue = "Black" },
                new ProductOption { Id = "jacket-red-s", ProductId = "winter-jacket", OptionType = "Color", OptionValue = "Red" },
                new ProductOption { Id = "jacket-red-m", ProductId = "winter-jacket", OptionType = "Color", OptionValue = "Red" }
            };

            // Act
            foreach (var option in productOptions)
            {
                _service.Add(option);
            }

            var allOptions = _service.GetAll();
            var jacketOptions = allOptions.Where(o => o.ProductId == "winter-jacket").ToList();
            var blackVariants = jacketOptions.Where(o => o.OptionValue == "Black").ToList();

            // Assert
            allOptions.Should().HaveCount(4);
            jacketOptions.Should().HaveCount(4);
            blackVariants.Should().HaveCount(2);
        }

        #endregion
    }
}
