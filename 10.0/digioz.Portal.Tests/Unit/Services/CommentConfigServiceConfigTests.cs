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
    /// Unit tests for CommentConfigService - Comment configuration management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Comments")]
    public class CommentConfigServiceConfigTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private CommentConfigService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new CommentConfigService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsCommentConfig()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "config-1",
                ReferenceType = "Article",
                ReferenceId = "article-123",
                ReferenceTitle = "My Article Title",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.CommentConfigs.Add(config);
            _context.SaveChanges();

            // Act
            var result = _service.Get("config-1");

            // Assert
            result.Should().NotBeNull();
            result!.ReferenceType.Should().Be("Article");
            result.ReferenceTitle.Should().Be("My Article Title");
            result.Visible.Should().BeTrue();
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

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleConfigs_ReturnsAll()
        {
            // Arrange
            _context.CommentConfigs.AddRange(
                new CommentConfig { Id = "1", ReferenceType = "Article", ReferenceId = "article-1", ReferenceTitle = "Article 1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "2", ReferenceType = "News", ReferenceId = "news-1", ReferenceTitle = "News 1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "3", ReferenceType = "Picture", ReferenceId = "pic-1", ReferenceTitle = "Picture 1", Visible = false, Timestamp = DateTime.UtcNow }
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
        public void GetAll_DeliveredInConsistentOrder()
        {
            // Arrange
            _context.CommentConfigs.AddRange(
                new CommentConfig { Id = "1", ReferenceType = "Article", ReferenceId = "a1", ReferenceTitle = "A1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "2", ReferenceType = "News", ReferenceId = "n1", ReferenceTitle = "N1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "3", ReferenceType = "Picture", ReferenceId = "p1", ReferenceTitle = "P1", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results1 = _service.GetAll();
            var results2 = _service.GetAll();

            // Assert
            results1.Select(c => c.Id).SequenceEqual(results2.Select(c => c.Id)).Should().BeTrue();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidConfig_AddsToDatabase()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "new-config",
                ReferenceType = "Article",
                ReferenceId = "article-456",
                ReferenceTitle = "New Article",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(config);

            // Assert
            var saved = _context.CommentConfigs.Find("new-config");
            saved.Should().NotBeNull();
            saved!.ReferenceType.Should().Be("Article");
        }

        [Test]
        public void Add_WithInvisibleConfig_Saves()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "invisible-config",
                ReferenceType = "Article",
                ReferenceId = "article-789",
                ReferenceTitle = "Hidden Article",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(config);

            // Assert
            var saved = _context.CommentConfigs.Find("invisible-config");
            saved!.Visible.Should().BeFalse();
        }

        [Test]
        public void Add_MultipleConfigs_AllAreSaved()
        {
            // Arrange
            var configs = new[]
            {
                new CommentConfig { Id = "1", ReferenceType = "Article", ReferenceId = "a1", ReferenceTitle = "A1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "2", ReferenceType = "News", ReferenceId = "n1", ReferenceTitle = "N1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "3", ReferenceType = "Picture", ReferenceId = "p1", ReferenceTitle = "P1", Visible = false, Timestamp = DateTime.UtcNow }
            };

            // Act
            foreach (var config in configs)
            {
                _service.Add(config);
            }

            // Assert
            _context.CommentConfigs.Should().HaveCount(3);
        }

        [Test]
        public void Add_WithoutTimestamp_Saves()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "no-timestamp-config",
                ReferenceType = "Article",
                ReferenceId = "article-999",
                ReferenceTitle = "No Timestamp Article",
                Visible = true,
                Timestamp = null
            };

            // Act
            _service.Add(config);

            // Assert
            var saved = _context.CommentConfigs.Find("no-timestamp-config");
            saved!.Timestamp.Should().BeNull();
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingConfig_UpdatesInDatabase()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "config-1",
                ReferenceType = "Article",
                ReferenceId = "article-old",
                ReferenceTitle = "Old Title",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };
            _context.CommentConfigs.Add(config);
            _context.SaveChanges();

            // Act
            config.ReferenceId = "article-new";
            config.ReferenceTitle = "New Title";
            config.Visible = true;
            _service.Update(config);

            // Assert
            var updated = _context.CommentConfigs.Find("config-1");
            updated!.ReferenceId.Should().Be("article-new");
            updated.ReferenceTitle.Should().Be("New Title");
            updated.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_ChangeVisibility_Updates()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "config-1",
                ReferenceType = "Article",
                ReferenceId = "article-1",
                ReferenceTitle = "Article Title",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };
            _context.CommentConfigs.Add(config);
            _context.SaveChanges();

            // Act
            config.Visible = true;
            _service.Update(config);

            // Assert
            var updated = _context.CommentConfigs.Find("config-1");
            updated!.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_DoesNotAffectOtherConfigs()
        {
            // Arrange
            _context.CommentConfigs.AddRange(
                new CommentConfig { Id = "1", ReferenceType = "Article", ReferenceId = "a1", ReferenceTitle = "A1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "2", ReferenceType = "News", ReferenceId = "n1", ReferenceTitle = "N1", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            var config1 = _context.CommentConfigs.Find("1");
            config1!.ReferenceTitle = "Updated A1";

            // Act
            _service.Update(config1);

            // Assert
            _context.CommentConfigs.Find("2")!.ReferenceTitle.Should().Be("N1");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "config-to-delete",
                ReferenceType = "Article",
                ReferenceId = "article-1",
                ReferenceTitle = "Delete Me",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.CommentConfigs.Add(config);
            _context.SaveChanges();

            // Act
            _service.Delete("config-to-delete");

            // Assert
            var deleted = _context.CommentConfigs.Find("config-to-delete");
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
            _context.CommentConfigs.AddRange(
                new CommentConfig { Id = "1", ReferenceType = "Article", ReferenceId = "a1", ReferenceTitle = "A1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "2", ReferenceType = "News", ReferenceId = "n1", ReferenceTitle = "N1", Visible = true, Timestamp = DateTime.UtcNow },
                new CommentConfig { Id = "3", ReferenceType = "Picture", ReferenceId = "p1", ReferenceTitle = "P1", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("2");

            // Assert
            _context.CommentConfigs.Should().HaveCount(2);
            _context.CommentConfigs.Find("2").Should().BeNull();
            _context.CommentConfigs.Find("1").Should().NotBeNull();
            _context.CommentConfigs.Find("3").Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void CommentConfig_FullLifecycle()
        {
            // Arrange
            var config = new CommentConfig
            {
                Id = "lifecycle-config",
                ReferenceType = "Article",
                ReferenceId = "article-lifecycle",
                ReferenceTitle = "Lifecycle Article",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };

            // Act - Add
            _service.Add(config);
            var added = _service.Get("lifecycle-config");
            added.Should().NotBeNull();

            // Act - Update
            added!.ReferenceTitle = "Updated Lifecycle Article";
            added.Visible = true;
            _service.Update(added);
            var updated = _service.Get("lifecycle-config");
            updated!.ReferenceTitle.Should().Be("Updated Lifecycle Article");
            updated.Visible.Should().BeTrue();

            // Act - Delete
            _service.Delete("lifecycle-config");
            var deleted = _service.Get("lifecycle-config");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageMultipleReferenceTypes()
        {
            // Arrange
            var referenceTypes = new[] { "Article", "News", "Picture", "Video" };

            // Act - Create configurations for different reference types
            int id = 1;
            foreach (var type in referenceTypes)
            {
                _service.Add(new CommentConfig
                {
                    Id = $"config-{id}",
                    ReferenceType = type,
                    ReferenceId = $"{type.ToLower()}-1",
                    ReferenceTitle = $"Sample {type}",
                    Visible = id > 2,
                    Timestamp = DateTime.UtcNow
                });
                id++;
            }

            // Act - Retrieve and verify
            var allConfigs = _service.GetAll();
            var visibleCount = allConfigs.Count(c => c.Visible);

            // Assert
            allConfigs.Should().HaveCount(4);
            visibleCount.Should().Be(2);
        }

        [Test]
        public void CommentConfig_CanToggleVisibility()
        {
            // Arrange
            _service.Add(new CommentConfig
            {
                Id = "toggle-1",
                ReferenceType = "Article",
                ReferenceId = "article-1",
                ReferenceTitle = "Test Article",
                Visible = false,
                Timestamp = DateTime.UtcNow
            });

            // Act - Enable comments
            var config = _service.Get("toggle-1");
            config!.Visible = true;
            _service.Update(config);

            // Assert
            var updated = _service.Get("toggle-1");
            updated!.Visible.Should().BeTrue();

            // Act - Disable comments again
            config = _service.Get("toggle-1");
            config!.Visible = false;
            _service.Update(config);

            // Assert
            var disabled = _service.Get("toggle-1");
            disabled!.Visible.Should().BeFalse();
        }

        [Test]
        public void TrackCommentSettings_ByReferenceType()
        {
            // Arrange
            var articleConfig = new CommentConfig
            {
                Id = "article-settings",
                ReferenceType = "Article",
                ReferenceId = "article-123",
                ReferenceTitle = "My Article",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            var newsConfig = new CommentConfig
            {
                Id = "news-settings",
                ReferenceType = "News",
                ReferenceId = "news-456",
                ReferenceTitle = "Latest News",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(articleConfig);
            _service.Add(newsConfig);
            var allConfigs = _service.GetAll();

            // Assert
            var enabledConfigs = allConfigs.Where(c => c.Visible).ToList();
            var disabledConfigs = allConfigs.Where(c => !c.Visible).ToList();
            enabledConfigs.Should().HaveCount(1);
            disabledConfigs.Should().HaveCount(1);
        }

        #endregion
    }
}
