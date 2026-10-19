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
    /// Unit tests for ModuleService - Content module/widget management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Content")]
    public class ModuleServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ModuleService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ModuleService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsModule()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                Title = "Recent Posts",
                Location = "Sidebar",
                Body = "RecentPosts",
                Visible = true,
                DisplayInBox = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Modules.Add(module);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Recent Posts");
            result.Location.Should().Be("Sidebar");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleModules_ReturnsAll()
        {
            // Arrange
            _context.Modules.AddRange(
                new Module { Id = 1, Title = "Module 1", Location = "Left", Body = "M1", Visible = true, DisplayInBox = true, Timestamp = DateTime.UtcNow },
                new Module { Id = 2, Title = "Module 2", Location = "Right", Body = "M2", Visible = true, DisplayInBox = false, Timestamp = DateTime.UtcNow },
                new Module { Id = 3, Title = "Module 3", Location = "Left", Body = "M3", Visible = false, DisplayInBox = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoModules_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidModule_AddsToDatabase()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                Title = "Top Posts",
                Location = "Homepage",
                Body = "TopPosts",
                Visible = true,
                DisplayInBox = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(module);

            // Assert
            var saved = _context.Modules.Find(1);
            saved.Should().NotBeNull();
            saved!.Title.Should().Be("Top Posts");
        }

        [Test]
        public void Add_WithUserId_SavesUserId()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                UserId = "user123",
                Title = "User Module",
                Location = "Profile",
                Body = "UserModule",
                Visible = true,
                DisplayInBox = false,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(module);

            // Assert
            var saved = _context.Modules.Find(1);
            saved!.UserId.Should().Be("user123");
        }

        [Test]
        public void Add_MultipleModules_AllAreSaved()
        {
            // Arrange
            var modules = new[]
            {
                new Module { Id = 1, Title = "M1", Location = "L1", Body = "B1", Visible = true, DisplayInBox = true, Timestamp = DateTime.UtcNow },
                new Module { Id = 2, Title = "M2", Location = "L2", Body = "B2", Visible = true, DisplayInBox = false, Timestamp = DateTime.UtcNow },
                new Module { Id = 3, Title = "M3", Location = "L3", Body = "B3", Visible = false, DisplayInBox = true, Timestamp = DateTime.UtcNow }
            };

            // Act
            foreach (var module in modules)
            {
                _service.Add(module);
            }

            // Assert
            _context.Modules.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingModule_UpdatesInDatabase()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                Title = "Original Title",
                Location = "Left",
                Body = "Original",
                Visible = false,
                DisplayInBox = false,
                Timestamp = DateTime.UtcNow
            };
            _context.Modules.Add(module);
            _context.SaveChanges();

            // Act
            module.Title = "Updated Title";
            module.Body = "Updated";
            module.Visible = true;
            module.DisplayInBox = true;
            _service.Update(module);

            // Assert
            var updated = _context.Modules.Find(1);
            updated!.Title.Should().Be("Updated Title");
            updated.Body.Should().Be("Updated");
            updated.Visible.Should().BeTrue();
            updated.DisplayInBox.Should().BeTrue();
        }

        [Test]
        public void Update_ToggleVisibility_Updates()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                Title = "Module",
                Location = "Side",
                Body = "Body",
                Visible = true,
                DisplayInBox = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Modules.Add(module);
            _context.SaveChanges();

            // Act
            module.Visible = false;
            _service.Update(module);

            // Assert
            var updated = _context.Modules.Find(1);
            updated!.Visible.Should().BeFalse();
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                Title = "To Delete",
                Location = "Temp",
                Body = "Delete",
                Visible = true,
                DisplayInBox = false,
                Timestamp = DateTime.UtcNow
            };
            _context.Modules.Add(module);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Modules.Find(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(999);
            act.Should().NotThrow();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Module_FullLifecycle()
        {
            // Arrange
            var module = new Module
            {
                Id = 1,
                Title = "Lifecycle Module",
                Location = "Test",
                Body = "Initial",
                Visible = false,
                DisplayInBox = false,
                Timestamp = DateTime.UtcNow
            };

            // Act - Add
            _service.Add(module);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Title = "Updated Lifecycle Module";
            added.Visible = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Title.Should().Be("Updated Lifecycle Module");

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageSiteModules_WithMultipleLocations()
        {
            // Arrange - Create modules for different locations
            _service.Add(new Module { Id = 1, Title = "Left 1", Location = "Left", Body = "L1", Visible = true, DisplayInBox = true, Timestamp = DateTime.UtcNow });
            _service.Add(new Module { Id = 2, Title = "Left 2", Location = "Left", Body = "L2", Visible = true, DisplayInBox = true, Timestamp = DateTime.UtcNow });
            _service.Add(new Module { Id = 3, Title = "Right 1", Location = "Right", Body = "R1", Visible = true, DisplayInBox = true, Timestamp = DateTime.UtcNow });

            // Act
            var allModules = _service.GetAll();
            var leftModules = allModules.Where(m => m.Location == "Left").ToList();
            var rightModules = allModules.Where(m => m.Location == "Right").ToList();

            // Assert
            allModules.Should().HaveCount(3);
            leftModules.Should().HaveCount(2);
            rightModules.Should().HaveCount(1);
        }

        [Test]
        public void ModuleVisibility_CanBeControlled()
        {
            // Arrange
            _service.Add(new Module { Id = 1, Title = "Visible", Location = "Side", Body = "V", Visible = true, DisplayInBox = true, Timestamp = DateTime.UtcNow });
            _service.Add(new Module { Id = 2, Title = "Hidden", Location = "Side", Body = "H", Visible = false, DisplayInBox = true, Timestamp = DateTime.UtcNow });

            // Act
            var allModules = _service.GetAll();
            var visible = allModules.Where(m => m.Visible).ToList();
            var hidden = allModules.Where(m => !m.Visible).ToList();

            // Assert
            visible.Should().HaveCount(1);
            hidden.Should().HaveCount(1);
        }

        #endregion
    }
}
