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
    /// Unit tests for AnnouncementService - System announcements management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    public class AnnouncementServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AnnouncementService _service;

        [SetUp]
        public void Setup()
        {
            // Use In-Memory database for isolated tests
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AnnouncementService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsAnnouncement()
        {
            // Arrange
            var announcement = new Announcement
            {
                Id = 1,
                Title = "Important Announcement",
                Body = "This is an important message",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Announcements.Add(announcement);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Important Announcement");
            result.Body.Should().Be("This is an important message");
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
        public void GetAll_WithMultipleAnnouncements_ReturnsAll()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Announcement 1", Body = "Body 1", Visible = true, Timestamp = DateTime.UtcNow.AddDays(-2) },
                new Announcement { Id = 2, Title = "Announcement 2", Body = "Body 2", Visible = true, Timestamp = DateTime.UtcNow.AddDays(-1) },
                new Announcement { Id = 3, Title = "Announcement 3", Body = "Body 3", Visible = false, Timestamp = DateTime.UtcNow }
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

        #region GetVisible Tests

        [Test]
        public void GetVisible_WithVisibleAnnouncements_ReturnsOnlyVisible()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Visible 1", Body = "Body 1", Visible = true, Timestamp = DateTime.UtcNow.AddDays(-2) },
                new Announcement { Id = 2, Title = "Visible 2", Body = "Body 2", Visible = true, Timestamp = DateTime.UtcNow.AddDays(-1) },
                new Announcement { Id = 3, Title = "Hidden", Body = "Body 3", Visible = false, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetVisible(10);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(a => a.Visible.Should().BeTrue());
        }

        [Test]
        public void GetVisible_WithCountLimit_ReturnsOnlyRequestedCount()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.Announcements.Add(new Announcement
                {
                    Id = i,
                    Title = $"Announcement {i}",
                    Body = $"Body {i}",
                    Visible = true,
                    Timestamp = DateTime.UtcNow.AddDays(-i)
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetVisible(5);

            // Assert
            results.Should().HaveCount(5);
        }

        [Test]
        public void GetVisible_IsOrderedByTimestampDescending()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Old", Body = "Body", Visible = true, Timestamp = DateTime.UtcNow.AddDays(-10) },
                new Announcement { Id = 2, Title = "New", Body = "Body", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetVisible(10);

            // Assert
            results.First().Title.Should().Be("New");
            results.Last().Title.Should().Be("Old");
        }

        #endregion

        #region GetPagedVisible Tests

        [Test]
        public void GetPagedVisible_WithValidPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                _context.Announcements.Add(new Announcement
                {
                    Id = i,
                    Title = $"Announcement {i}",
                    Body = $"Body {i}",
                    Visible = true,
                    Timestamp = DateTime.UtcNow.AddDays(-i)
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetPagedVisible(2, 5, out int totalCount);

            // Assert
            results.Should().HaveCount(5);
            totalCount.Should().Be(15);
        }

        [Test]
        public void GetPagedVisible_FiltersByVisibility()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Visible 1", Body = "Body", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 2, Title = "Hidden", Body = "Body", Visible = false, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 3, Title = "Visible 2", Body = "Body", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetPagedVisible(1, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
            results.Should().AllSatisfy(a => a.Visible.Should().BeTrue());
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidAnnouncement_AddsToDatabase()
        {
            // Arrange
            var announcement = new Announcement
            {
                Title = "New Announcement",
                Body = "New content",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(announcement);

            // Assert
            var saved = _context.Announcements.FirstOrDefault(a => a.Title == "New Announcement");
            saved.Should().NotBeNull();
            saved!.Body.Should().Be("New content");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingAnnouncement_UpdatesInDatabase()
        {
            // Arrange
            var announcement = new Announcement
            {
                Id = 1,
                Title = "Original Title",
                Body = "Original Body",
                Visible = true
            };
            _context.Announcements.Add(announcement);
            _context.SaveChanges();

            // Act
            announcement.Title = "Updated Title";
            announcement.Body = "Updated Body";
            _service.Update(announcement);

            // Assert
            var updated = _context.Announcements.Find(1);
            updated!.Title.Should().Be("Updated Title");
            updated.Body.Should().Be("Updated Body");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var announcement = new Announcement
            {
                Id = 1,
                Title = "To Delete",
                Body = "Body",
                Visible = true
            };
            _context.Announcements.Add(announcement);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Announcements.Find(1);
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

        #region Search Tests

        [Test]
        public void Search_WithMatchingTerm_ReturnsMatchingAnnouncements()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Important Update", Body = "System maintenance", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 2, Title = "Event Announcement", Body = "New feature launch", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 3, Title = "Critical Update", Body = "Security patch", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("Update", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(a => a.Title.Should().Contain("Update"));
            totalCount.Should().Be(2);
        }

        [Test]
        public void Search_WithEmptyTerm_ReturnsAllVisibleAnnouncements()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Announcement 1", Body = "Body", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 2, Title = "Announcement 2", Body = "Body", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 3, Title = "Hidden", Body = "Body", Visible = false, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Test]
        public void Search_SearchInTitle_ReturnsMatchingByTitle()
        {
            // Arrange
            _context.Announcements.AddRange(
                new Announcement { Id = 1, Title = "System Maintenance Alert", Body = "Details", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 2, Title = "Feature Announcement", Body = "Details", Visible = true, Timestamp = DateTime.UtcNow },
                new Announcement { Id = 3, Title = "Update Info", Body = "Maintenance scheduled", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("Maintenance", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
        }

        [Test]
        public void Search_WithNonMatchingTerm_ReturnsEmptyList()
        {
            // Arrange
            _context.Announcements.Add(new Announcement
            {
                Id = 1,
                Title = "Test Announcement",
                Body = "Test content",
                Visible = true,
                Timestamp = DateTime.UtcNow
            });
            _context.SaveChanges();

            // Act
            var results = _service.Search("nonexistent", 0, 10, out int totalCount);

            // Assert
            results.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Test]
        public void Search_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 20; i++)
            {
                _context.Announcements.Add(new Announcement
                {
                    Id = i,
                    Title = $"Announcement {i}",
                    Body = "Content",
                    Visible = true,
                    Timestamp = DateTime.UtcNow.AddDays(-i)
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.Search("", 10, 5, out int totalCount);

            // Assert
            results.Should().HaveCount(5);
            totalCount.Should().Be(20);
        }

        #endregion
    }
}
