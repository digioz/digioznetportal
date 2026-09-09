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
    /// Unit tests for VisitorSessionService - User session and visitor tracking
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Analytics")]
    public class VisitorSessionServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private VisitorSessionService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new VisitorSessionService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsVisitorSession()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var session = new VisitorSession
            {
                Id = 1,
                IpAddress = "192.168.1.1",
                PageUrl = "/home",
                SessionId = "session-123",
                Username = "guest",
                DateCreated = now,
                DateModified = now
            };
            _context.VisitorSessions.Add(session);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.IpAddress.Should().Be("192.168.1.1");
            result.SessionId.Should().Be("session-123");
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
        public void GetAll_WithMultipleSessions_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.VisitorSessions.AddRange(
                new VisitorSession { Id = 1, IpAddress = "192.168.1.1", PageUrl = "/home", SessionId = "s1", Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 2, IpAddress = "192.168.1.2", PageUrl = "/about", SessionId = "s2", Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 3, IpAddress = "192.168.1.3", PageUrl = "/products", SessionId = "s3", Username = "user-1", DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoSessions_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetAllGreaterThan Tests

        [Test]
        public void GetAllGreaterThan_WithValidDateTime_ReturnsMostRecentSessions()
        {
            // Arrange
            var baseTime = DateTime.UtcNow;
            _context.VisitorSessions.AddRange(
                new VisitorSession { Id = 1, IpAddress = "1.1.1.1", PageUrl = "/p1", SessionId = "s1", Username = "guest", DateCreated = baseTime.AddHours(-2), DateModified = baseTime.AddHours(-2) },
                new VisitorSession { Id = 2, IpAddress = "1.1.1.2", PageUrl = "/p2", SessionId = "s2", Username = "guest", DateCreated = baseTime.AddHours(-1), DateModified = baseTime.AddHours(-1) },
                new VisitorSession { Id = 3, IpAddress = "1.1.1.3", PageUrl = "/p3", SessionId = "s3", Username = "guest", DateCreated = baseTime, DateModified = baseTime }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAllGreaterThan(baseTime.AddHours(-1).AddMinutes(1));

            // Assert
            results.Should().HaveCount(1);
            results[0].Id.Should().Be(3);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidSession_AddsToDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var session = new VisitorSession
            {
                Id = 1,
                IpAddress = "10.0.0.1",
                PageUrl = "/page",
                SessionId = "new-session",
                Username = "user",
                DateCreated = now,
                DateModified = now
            };

            // Act
            _service.Add(session);

            // Assert
            var saved = _context.VisitorSessions.Find(1);
            saved.Should().NotBeNull();
            saved!.SessionId.Should().Be("new-session");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingSession_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var session = new VisitorSession
            {
                Id = 1,
                IpAddress = "192.168.1.1",
                PageUrl = "/home",
                SessionId = "session-1",
                Username = "guest",
                DateCreated = now,
                DateModified = now
            };
            _context.VisitorSessions.Add(session);
            _context.SaveChanges();

            // Act
            session.PageUrl = "/updated-page";
            session.DateModified = now.AddSeconds(10);
            _service.Update(session);

            // Assert
            var updated = _context.VisitorSessions.Find(1);
            updated!.PageUrl.Should().Be("/updated-page");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var session = new VisitorSession
            {
                Id = 1,
                IpAddress = "192.168.1.1",
                PageUrl = "/delete",
                SessionId = "delete-session",
                Username = "guest",
                DateCreated = now,
                DateModified = now
            };
            _context.VisitorSessions.Add(session);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.VisitorSessions.Find(1);
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

        #region GetPaged Tests

        [Test]
        public void GetPaged_WithValidPageParameters_ReturnsPaged()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.VisitorSessions.AddRange(
                new VisitorSession { Id = 1, IpAddress = "1.1.1.1", PageUrl = "/p1", SessionId = "s1", Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 2, IpAddress = "1.1.1.2", PageUrl = "/p2", SessionId = "s2", Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 3, IpAddress = "1.1.1.3", PageUrl = "/p3", SessionId = "s3", Username = "guest", DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            var page1 = _service.GetPaged(1, 2);

            // Assert
            page1.Should().HaveCount(2);
        }

        #endregion

        #region CountAll Tests

        [Test]
        public void CountAll_WithMultipleSessions_ReturnsTotal()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.VisitorSessions.AddRange(
                new VisitorSession { Id = 1, IpAddress = "1.1.1.1", PageUrl = "/p1", SessionId = "s1", Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 2, IpAddress = "1.1.1.2", PageUrl = "/p2", SessionId = "s2", Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 3, IpAddress = "1.1.1.3", PageUrl = "/p3", SessionId = "s3", Username = "guest", DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountAll();

            // Assert
            count.Should().Be(3);
        }

        #endregion

        #region GetByDateRange Tests

        [Test]
        public void GetByDateRange_WithValidRange_ReturnsSessionsInRange()
        {
            // Arrange
            var baseTime = DateTime.UtcNow;
            _context.VisitorSessions.AddRange(
                new VisitorSession { Id = 1, IpAddress = "1.1.1.1", PageUrl = "/p1", SessionId = "s1", Username = "guest", DateCreated = baseTime.AddDays(-2), DateModified = baseTime.AddDays(-2) },
                new VisitorSession { Id = 2, IpAddress = "1.1.1.2", PageUrl = "/p2", SessionId = "s2", Username = "guest", DateCreated = baseTime.AddDays(-1), DateModified = baseTime.AddDays(-1) },
                new VisitorSession { Id = 3, IpAddress = "1.1.1.3", PageUrl = "/p3", SessionId = "s3", Username = "guest", DateCreated = baseTime, DateModified = baseTime }
            );
            _context.SaveChanges();

            // Act
            var range = _service.GetByDateRange(baseTime.AddDays(-1.5), baseTime.AddHours(1));

            // Assert
            range.Should().HaveCount(2);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void VisitorSession_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var session = new VisitorSession
            {
                Id = 1,
                IpAddress = "192.168.1.100",
                PageUrl = "/initial",
                SessionId = "lifecycle-session",
                Username = "guest",
                DateCreated = now,
                DateModified = now
            };

            // Act - Add
            _service.Add(session);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.PageUrl = "/updated-page";
            added.DateModified = now.AddMinutes(5);
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.PageUrl.Should().Be("/updated-page");

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void TrackVisitorSessions_MultiplePages()
        {
            // Arrange - Simulate a visitor browsing multiple pages
            var now = DateTime.UtcNow;
            var visitorIp = "203.0.113.42";
            var sessionId = "visitor-session-001";

            var pageViews = new[]
            {
                new VisitorSession { Id = 1, IpAddress = visitorIp, PageUrl = "/", SessionId = sessionId, Username = "guest", DateCreated = now, DateModified = now },
                new VisitorSession { Id = 2, IpAddress = visitorIp, PageUrl = "/products", SessionId = sessionId, Username = "guest", DateCreated = now.AddSeconds(5), DateModified = now.AddSeconds(5) },
                new VisitorSession { Id = 3, IpAddress = visitorIp, PageUrl = "/product/123", SessionId = sessionId, Username = "guest", DateCreated = now.AddSeconds(10), DateModified = now.AddSeconds(10) },
                new VisitorSession { Id = 4, IpAddress = visitorIp, PageUrl = "/cart", SessionId = sessionId, Username = "guest", DateCreated = now.AddSeconds(20), DateModified = now.AddSeconds(20) }
            };

            // Act
            foreach (var view in pageViews)
            {
                _service.Add(view);
            }

            var allSessions = _service.GetAll();
            var visitorSessions = allSessions.Where(s => s.IpAddress == visitorIp).ToList();
            var sessionCount = _service.CountAll();

            // Assert
            allSessions.Should().HaveCount(4);
            visitorSessions.Should().HaveCount(4);
            sessionCount.Should().Be(4);
        }

        #endregion
    }
}
