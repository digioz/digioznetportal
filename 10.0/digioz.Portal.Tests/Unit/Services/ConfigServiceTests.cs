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
    /// Unit tests for ConfigService - Application configuration management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Configuration")]
    public class ConfigServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ConfigService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ConfigService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsConfig()
        {
            // Arrange
            var config = new Config
            {
                Id = "config-1",
                ConfigKey = "app-name",
                ConfigValue = "My Portal",
                IsEncrypted = false
            };
            _context.Configs.Add(config);
            _context.SaveChanges();

            // Act
            var result = _service.Get("config-1");

            // Assert
            result.Should().NotBeNull();
            result!.ConfigKey.Should().Be("app-name");
            result.ConfigValue.Should().Be("My Portal");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent-id");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_WithNullId_ReturnsNull()
        {
            // Act
            var result = _service.Get(null);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByKey Tests

        [Test]
        public void GetByKey_WithValidKey_ReturnsConfig()
        {
            // Arrange
            var config = new Config
            {
                Id = "config-1",
                ConfigKey = "smtp-host",
                ConfigValue = "smtp.example.com",
                IsEncrypted = false
            };
            _context.Configs.Add(config);
            _context.SaveChanges();

            // Act
            var result = _service.GetByKey("smtp-host");

            // Assert
            result.Should().NotBeNull();
            result!.ConfigValue.Should().Be("smtp.example.com");
        }

        [Test]
        public void GetByKey_WithInvalidKey_ReturnsNull()
        {
            // Act
            var result = _service.GetByKey("nonexistent-key");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByKey_WithNullKey_ReturnsNull()
        {
            // Act
            var result = _service.GetByKey(null);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleConfigs_ReturnsAll()
        {
            // Arrange
            _context.Configs.AddRange(
                new Config { Id = "1", ConfigKey = "app-name", ConfigValue = "Portal", IsEncrypted = false },
                new Config { Id = "2", ConfigKey = "app-version", ConfigValue = "1.0.0", IsEncrypted = false },
                new Config { Id = "3", ConfigKey = "db-password", ConfigValue = "encrypted_pwd", IsEncrypted = true }
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

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidConfig_AddsToDatabase()
        {
            // Arrange
            var config = new Config
            {
                Id = "new-config",
                ConfigKey = "site-title",
                ConfigValue = "My Awesome Site",
                IsEncrypted = false
            };

            // Act
            _service.Add(config);

            // Assert
            var saved = _context.Configs.Find("new-config");
            saved.Should().NotBeNull();
            saved!.ConfigKey.Should().Be("site-title");
        }

        [Test]
        public void Add_WithEncryptedConfig_SavesEncryptionFlag()
        {
            // Arrange
            var config = new Config
            {
                Id = "api-key-config",
                ConfigKey = "api-key",
                ConfigValue = "secret_key_xyz",
                IsEncrypted = true
            };

            // Act
            _service.Add(config);

            // Assert
            var saved = _context.Configs.Find("api-key-config");
            saved!.IsEncrypted.Should().BeTrue();
        }

        [Test]
        public void Add_MultipleConfigs_AllAreSaved()
        {
            // Arrange
            var configs = new[]
            {
                new Config { Id = "1", ConfigKey = "key1", ConfigValue = "value1", IsEncrypted = false },
                new Config { Id = "2", ConfigKey = "key2", ConfigValue = "value2", IsEncrypted = false },
                new Config { Id = "3", ConfigKey = "key3", ConfigValue = "value3", IsEncrypted = true }
            };

            // Act
            foreach (var config in configs)
            {
                _service.Add(config);
            }

            // Assert
            _context.Configs.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingConfig_UpdatesInDatabase()
        {
            // Arrange
            var config = new Config
            {
                Id = "config-1",
                ConfigKey = "old-key",
                ConfigValue = "old-value",
                IsEncrypted = false
            };
            _context.Configs.Add(config);
            _context.SaveChanges();

            // Act
            config.ConfigKey = "new-key";
            config.ConfigValue = "new-value";
            _service.Update(config);

            // Assert
            var updated = _context.Configs.Find("config-1");
            updated!.ConfigKey.Should().Be("new-key");
            updated.ConfigValue.Should().Be("new-value");
        }

        [Test]
        public void Update_WithEncryptionFlagChange_Updates()
        {
            // Arrange
            var config = new Config
            {
                Id = "config-1",
                ConfigKey = "password",
                ConfigValue = "plaintext_pwd",
                IsEncrypted = false
            };
            _context.Configs.Add(config);
            _context.SaveChanges();

            // Act
            config.IsEncrypted = true;
            _service.Update(config);

            // Assert
            var updated = _context.Configs.Find("config-1");
            updated!.IsEncrypted.Should().BeTrue();
        }

        [Test]
        public void Update_DoesNotAffectOtherConfigs()
        {
            // Arrange
            _context.Configs.AddRange(
                new Config { Id = "1", ConfigKey = "key1", ConfigValue = "value1", IsEncrypted = false },
                new Config { Id = "2", ConfigKey = "key2", ConfigValue = "value2", IsEncrypted = false }
            );
            _context.SaveChanges();

            var config1 = _context.Configs.Find("1");
            config1!.ConfigValue = "updated-value";

            // Act
            _service.Update(config1);

            // Assert
            _context.Configs.Find("2")!.ConfigValue.Should().Be("value2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var config = new Config
            {
                Id = "config-1",
                ConfigKey = "temp-key",
                ConfigValue = "temp-value",
                IsEncrypted = false
            };
            _context.Configs.Add(config);
            _context.SaveChanges();

            // Act
            _service.Delete("config-1");

            // Assert
            var deleted = _context.Configs.Find("config-1");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent-id");
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_WithNullId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(null);
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesCorrectConfigWhenMultipleExist()
        {
            // Arrange
            _context.Configs.AddRange(
                new Config { Id = "1", ConfigKey = "key1", ConfigValue = "value1", IsEncrypted = false },
                new Config { Id = "2", ConfigKey = "key2", ConfigValue = "value2", IsEncrypted = false },
                new Config { Id = "3", ConfigKey = "key3", ConfigValue = "value3", IsEncrypted = false }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("2");

            // Assert
            _context.Configs.Should().HaveCount(2);
            _context.Configs.Find("2").Should().BeNull();
            _context.Configs.Find("1").Should().NotBeNull();
            _context.Configs.Find("3").Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Config_FullLifecycle()
        {
            // Arrange
            var config = new Config
            {
                Id = "lifecycle-config",
                ConfigKey = "lifecycle-key",
                ConfigValue = "original-value",
                IsEncrypted = false
            };

            // Act - Add
            _service.Add(config);
            var added = _service.Get("lifecycle-config");
            added.Should().NotBeNull();

            // Act - Update
            added!.ConfigValue = "updated-value";
            _service.Update(added);
            var updated = _service.Get("lifecycle-config");
            updated!.ConfigValue.Should().Be("updated-value");

            // Act - Delete
            _service.Delete("lifecycle-config");
            var deleted = _service.Get("lifecycle-config");
            deleted.Should().BeNull();
        }

        [Test]
        public void AppSettings_CanBeManaged()
        {
            // Arrange
            var appConfigs = new[]
            {
                new Config { Id = "app1", ConfigKey = "app-name", ConfigValue = "My App", IsEncrypted = false },
                new Config { Id = "app2", ConfigKey = "app-version", ConfigValue = "2.1.0", IsEncrypted = false },
                new Config { Id = "app3", ConfigKey = "support-email", ConfigValue = "support@app.com", IsEncrypted = false }
            };

            // Act
            foreach (var config in appConfigs)
            {
                _service.Add(config);
            }

            var allConfigs = _service.GetAll();
            var name = _service.GetByKey("app-name");
            var version = _service.GetByKey("app-version");

            // Assert
            allConfigs.Should().HaveCount(3);
            name!.ConfigValue.Should().Be("My App");
            version!.ConfigValue.Should().Be("2.1.0");
        }

        [Test]
        public void EncryptedConfigs_AreIdentified()
        {
            // Arrange
            _service.Add(new Config { Id = "1", ConfigKey = "username", ConfigValue = "admin", IsEncrypted = false });
            _service.Add(new Config { Id = "2", ConfigKey = "password", ConfigValue = "pwd123", IsEncrypted = true });
            _service.Add(new Config { Id = "3", ConfigKey = "api-secret", ConfigValue = "secret", IsEncrypted = true });

            // Act
            var allConfigs = _service.GetAll();
            var encrypted = allConfigs.Where(c => c.IsEncrypted);

            // Assert
            encrypted.Should().HaveCount(2);
            encrypted.Should().AllSatisfy(c => c.IsEncrypted.Should().BeTrue());
        }

        #endregion
    }
}
