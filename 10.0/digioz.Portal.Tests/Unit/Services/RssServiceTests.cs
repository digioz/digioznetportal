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
    /// Unit tests for RssService - RSS feed management and syndication
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Content")]
    public class RssServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private RssService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new RssService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsRss()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rss = new Rss
            {
                Id = 1,
                Name = "Tech News Feed",
                Url = "https://technews.example.com/feed",
                MaxCount = 50,
                Timestamp = now
            };
            _context.Rsses.Add(rss);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Tech News Feed");
            result.Url.Should().Be("https://technews.example.com/feed");
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
        public void GetAll_WithMultipleRssFeeds_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Rsses.AddRange(
                new Rss { Id = 1, Name = "Tech News", Url = "https://tech.example.com/feed", MaxCount = 50, Timestamp = now },
                new Rss { Id = 2, Name = "Business News", Url = "https://business.example.com/feed", MaxCount = 30, Timestamp = now },
                new Rss { Id = 3, Name = "Science News", Url = "https://science.example.com/feed", MaxCount = 40, Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoRssFeeds_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetPage Tests

        [Test]
        public void GetPage_WithValidPageParameters_ReturnsPaged()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Rsses.AddRange(
                new Rss { Id = 1, Name = "Feed 1", Url = "https://feed1.example.com", MaxCount = 50, Timestamp = now },
                new Rss { Id = 2, Name = "Feed 2", Url = "https://feed2.example.com", MaxCount = 50, Timestamp = now },
                new Rss { Id = 3, Name = "Feed 3", Url = "https://feed3.example.com", MaxCount = 50, Timestamp = now },
                new Rss { Id = 4, Name = "Feed 4", Url = "https://feed4.example.com", MaxCount = 50, Timestamp = now },
                new Rss { Id = 5, Name = "Feed 5", Url = "https://feed5.example.com", MaxCount = 50, Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var page1 = _service.GetPage(1, 2, out int totalCount);

            // Assert
            page1.Should().HaveCount(2);
            totalCount.Should().Be(5);
        }

        [Test]
        public void GetPage_WithMultiplePages_CorrectPaging()
        {
            // Arrange
            var now = DateTime.UtcNow;
            for (int i = 1; i <= 10; i++)
            {
                _context.Rsses.Add(new Rss { Id = i, Name = $"Feed {i}", Url = $"https://feed{i}.example.com", MaxCount = 50, Timestamp = now });
            }
            _context.SaveChanges();

            // Act
            var page1 = _service.GetPage(1, 3, out int total1);
            var page2 = _service.GetPage(2, 3, out int total2);

            // Assert
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(3);
            total1.Should().Be(10);
            total2.Should().Be(10);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidRss_AddsToDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rss = new Rss
            {
                Id = 1,
                Name = "New Feed",
                Url = "https://newfeed.example.com",
                MaxCount = 100,
                Timestamp = now
            };

            // Act
            _service.Add(rss);

            // Assert
            var saved = _context.Rsses.Find(1);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Feed");
        }

        [Test]
        public void Add_MultipleFeeds_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var feeds = new[]
            {
                new Rss { Id = 1, Name = "Feed 1", Url = "https://feed1.example.com", MaxCount = 50, Timestamp = now },
                new Rss { Id = 2, Name = "Feed 2", Url = "https://feed2.example.com", MaxCount = 60, Timestamp = now },
                new Rss { Id = 3, Name = "Feed 3", Url = "https://feed3.example.com", MaxCount = 70, Timestamp = now }
            };

            // Act
            foreach (var feed in feeds)
            {
                _service.Add(feed);
            }

            // Assert
            _context.Rsses.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingRss_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rss = new Rss
            {
                Id = 1,
                Name = "Original Name",
                Url = "https://original.example.com",
                MaxCount = 50,
                Timestamp = now
            };
            _context.Rsses.Add(rss);
            _context.SaveChanges();

            // Act
            rss.Name = "Updated Name";
            rss.MaxCount = 100;
            _service.Update(rss);

            // Assert
            var updated = _context.Rsses.Find(1);
            updated!.Name.Should().Be("Updated Name");
            updated.MaxCount.Should().Be(100);
        }

        [Test]
        public void Update_ChangeMaxCount_Updates()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rss = new Rss
            {
                Id = 1,
                Name = "Feed",
                Url = "https://feed.example.com",
                MaxCount = 50,
                Timestamp = now
            };
            _context.Rsses.Add(rss);
            _context.SaveChanges();

            // Act
            rss.MaxCount = 200;
            _service.Update(rss);

            // Assert
            var updated = _context.Rsses.Find(1);
            updated!.MaxCount.Should().Be(200);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rss = new Rss
            {
                Id = 1,
                Name = "To Delete",
                Url = "https://deleteme.example.com",
                MaxCount = 50,
                Timestamp = now
            };
            _context.Rsses.Add(rss);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Rsses.Find(1);
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
        public void RssFeed_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rss = new Rss
            {
                Id = 1,
                Name = "Lifecycle Feed",
                Url = "https://lifecycle.example.com/feed",
                MaxCount = 75,
                Timestamp = now
            };

            // Act - Add
            _service.Add(rss);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Feed";
            added.MaxCount = 150;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Name.Should().Be("Updated Lifecycle Feed");

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageFeedSyndication_MultipleFeeds()
        {
            // Arrange - Simulate managing multiple news feeds
            var now = DateTime.UtcNow;
            var feeds = new[]
            {
                new Rss { Id = 1, Name = "Breaking News", Url = "https://news.example.com/breaking", MaxCount = 50, Timestamp = now },
                new Rss { Id = 2, Name = "Technology", Url = "https://news.example.com/tech", MaxCount = 40, Timestamp = now },
                new Rss { Id = 3, Name = "Sports", Url = "https://news.example.com/sports", MaxCount = 30, Timestamp = now },
                new Rss { Id = 4, Name = "Entertainment", Url = "https://news.example.com/entertainment", MaxCount = 25, Timestamp = now }
            };

            // Act
            foreach (var feed in feeds)
            {
                _service.Add(feed);
            }

            var allFeeds = _service.GetAll();
            var news = _service.Get(1);

            // Assert
            allFeeds.Should().HaveCount(4);
            news!.Name.Should().Be("Breaking News");
        }

        [Test]
        public void PaginatedFeedList_ManageLargeFeeds()
        {
            // Arrange - Create many feeds for pagination testing
            var now = DateTime.UtcNow;
            for (int i = 1; i <= 25; i++)
            {
                var rss = new Rss { Id = i, Name = $"Feed {i}", Url = $"https://feed{i}.example.com", MaxCount = 50, Timestamp = now };
                _service.Add(rss);
            }

            // Act
            var page1 = _service.GetPage(1, 10, out int total);
            var page2 = _service.GetPage(2, 10, out int total2);
            var page3 = _service.GetPage(3, 10, out int total3);

            // Assert
            page1.Should().HaveCount(10);
            page2.Should().HaveCount(10);
            page3.Should().HaveCount(5);
            total.Should().Be(25);
        }

        #endregion
    }
}
