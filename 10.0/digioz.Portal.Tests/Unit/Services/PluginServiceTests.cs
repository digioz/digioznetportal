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
    /// Unit tests for PluginService - Plugin management (DLL extensions)
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Extensions")]
    public class PluginServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PluginService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PluginService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPlugin()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "ImageOptimizer",
                Dll = "ImageOptimizer.dll",
                IsEnabled = true
            };
            _context.Plugins.Add(plugin);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("ImageOptimizer");
            result.Dll.Should().Be("ImageOptimizer.dll");
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

        #region GetByName Tests

        [Test]
        public void GetByName_WithValidName_ReturnsPlugin()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "EmailValidator",
                Dll = "EmailValidator.dll",
                IsEnabled = true
            };
            _context.Plugins.Add(plugin);
            _context.SaveChanges();

            // Act
            var result = _service.GetByName("EmailValidator");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("EmailValidator");
        }

        [Test]
        public void GetByName_WithInvalidName_ReturnsNull()
        {
            // Act
            var result = _service.GetByName("NonExistentPlugin");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultiplePlugins_ReturnsAll()
        {
            // Arrange
            _context.Plugins.AddRange(
                new Plugin { Id = 1, Name = "Plugin1", Dll = "Plugin1.dll", IsEnabled = true },
                new Plugin { Id = 2, Name = "Plugin2", Dll = "Plugin2.dll", IsEnabled = false },
                new Plugin { Id = 3, Name = "Plugin3", Dll = "Plugin3.dll", IsEnabled = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoPlugins_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidPlugin_AddsToDatabase()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "TextProcessor",
                Dll = "TextProcessor.dll",
                IsEnabled = true
            };

            // Act
            _service.Add(plugin);

            // Assert
            var saved = _context.Plugins.Find(1);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("TextProcessor");
        }

        [Test]
        public void Add_WithDisabledPlugin_Saves()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "DisabledPlugin",
                Dll = "DisabledPlugin.dll",
                IsEnabled = false
            };

            // Act
            _service.Add(plugin);

            // Assert
            var saved = _context.Plugins.Find(1);
            saved!.IsEnabled.Should().BeFalse();
        }

        [Test]
        public void Add_MultiplePlugins_AllAreSaved()
        {
            // Arrange
            var plugins = new[]
            {
                new Plugin { Id = 1, Name = "P1", Dll = "P1.dll", IsEnabled = true },
                new Plugin { Id = 2, Name = "P2", Dll = "P2.dll", IsEnabled = false },
                new Plugin { Id = 3, Name = "P3", Dll = "P3.dll", IsEnabled = true }
            };

            // Act
            foreach (var plugin in plugins)
            {
                _service.Add(plugin);
            }

            // Assert
            _context.Plugins.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingPlugin_UpdatesInDatabase()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "OriginalName",
                Dll = "Original.dll",
                IsEnabled = false
            };
            _context.Plugins.Add(plugin);
            _context.SaveChanges();

            // Act
            plugin.Name = "UpdatedName";
            plugin.Dll = "Updated.dll";
            plugin.IsEnabled = true;
            _service.Update(plugin);

            // Assert
            var updated = _context.Plugins.Find(1);
            updated!.Name.Should().Be("UpdatedName");
            updated.Dll.Should().Be("Updated.dll");
            updated.IsEnabled.Should().BeTrue();
        }

        [Test]
        public void Update_ToggleEnabled_Updates()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "Toggle Plugin",
                Dll = "Toggle.dll",
                IsEnabled = true
            };
            _context.Plugins.Add(plugin);
            _context.SaveChanges();

            // Act
            plugin.IsEnabled = false;
            _service.Update(plugin);

            // Assert
            var updated = _context.Plugins.Find(1);
            updated!.IsEnabled.Should().BeFalse();
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "DeleteMe",
                Dll = "DeleteMe.dll",
                IsEnabled = false
            };
            _context.Plugins.Add(plugin);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Plugins.Find(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(999);
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesCorrectPluginWhenMultipleExist()
        {
            // Arrange
            _context.Plugins.AddRange(
                new Plugin { Id = 1, Name = "P1", Dll = "P1.dll", IsEnabled = true },
                new Plugin { Id = 2, Name = "P2", Dll = "P2.dll", IsEnabled = true },
                new Plugin { Id = 3, Name = "P3", Dll = "P3.dll", IsEnabled = false }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.Plugins.Should().HaveCount(2);
            _context.Plugins.Find(2).Should().BeNull();
            _context.Plugins.Find(1).Should().NotBeNull();
            _context.Plugins.Find(3).Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Plugin_FullLifecycle()
        {
            // Arrange
            var plugin = new Plugin
            {
                Id = 1,
                Name = "LifecyclePlugin",
                Dll = "Lifecycle.dll",
                IsEnabled = false
            };

            // Act - Add
            _service.Add(plugin);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "UpdatedLifecyclePlugin";
            added.IsEnabled = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Name.Should().Be("UpdatedLifecyclePlugin");
            updated.IsEnabled.Should().BeTrue();

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManagePlugins_EnableDisable()
        {
            // Arrange
            _service.Add(new Plugin { Id = 1, Name = "Enabled1", Dll = "E1.dll", IsEnabled = true });
            _service.Add(new Plugin { Id = 2, Name = "Disabled1", Dll = "D1.dll", IsEnabled = false });
            _service.Add(new Plugin { Id = 3, Name = "Enabled2", Dll = "E2.dll", IsEnabled = true });

            // Act
            var allPlugins = _service.GetAll();
            var enabledPlugins = allPlugins.Where(p => p.IsEnabled).ToList();
            var disabledPlugins = allPlugins.Where(p => !p.IsEnabled).ToList();

            // Assert
            allPlugins.Should().HaveCount(3);
            enabledPlugins.Should().HaveCount(2);
            disabledPlugins.Should().HaveCount(1);
        }

        [Test]
        public void SearchPlugin_ByName()
        {
            // Arrange
            _service.Add(new Plugin { Id = 1, Name = "ImageProcessor", Dll = "IP.dll", IsEnabled = true });
            _service.Add(new Plugin { Id = 2, Name = "TextAnalyzer", Dll = "TA.dll", IsEnabled = true });
            _service.Add(new Plugin { Id = 3, Name = "DataExporter", Dll = "DE.dll", IsEnabled = false });

            // Act
            var found = _service.GetByName("ImageProcessor");
            var notFound = _service.GetByName("VideoEncoder");

            // Assert
            found.Should().NotBeNull();
            found!.Dll.Should().Be("IP.dll");
            notFound.Should().BeNull();
        }

        #endregion
    }
}
