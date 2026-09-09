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
    /// Unit tests for MailingListSubscriberService - Email subscriber management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Marketing")]
    public class MailingListSubscriberServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private MailingListSubscriberService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new MailingListSubscriberService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsSubscriber()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "sub-1",
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                Status = true,
                DateCreated = now,
                DateModified = now
            };
            _context.MailingListSubscribers.Add(subscriber);
            _context.SaveChanges();

            // Act
            var result = _service.Get("sub-1");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("john@example.com");
            result.FirstName.Should().Be("John");
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
        public void GetAll_WithMultipleSubscribers_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.MailingListSubscribers.AddRange(
                new MailingListSubscriber { Id = "sub-1", Email = "alice@example.com", FirstName = "Alice", LastName = "Smith", Status = true, DateCreated = now, DateModified = now },
                new MailingListSubscriber { Id = "sub-2", Email = "bob@example.com", FirstName = "Bob", LastName = "Johnson", Status = true, DateCreated = now, DateModified = now },
                new MailingListSubscriber { Id = "sub-3", Email = "charlie@example.com", FirstName = "Charlie", LastName = "Brown", Status = false, DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoSubscribers_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidSubscriber_AddsToDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "new-sub",
                Email = "new@example.com",
                FirstName = "New",
                LastName = "Subscriber",
                Status = true,
                DateCreated = now,
                DateModified = now
            };

            // Act
            _service.Add(subscriber);

            // Assert
            var saved = _context.MailingListSubscribers.Find("new-sub");
            saved.Should().NotBeNull();
            saved!.Email.Should().Be("new@example.com");
        }

        [Test]
        public void Add_MultipleSubscribers_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscribers = new[]
            {
                new MailingListSubscriber { Id = "s1", Email = "s1@test.com", FirstName = "Sub", LastName = "One", Status = true, DateCreated = now, DateModified = now },
                new MailingListSubscriber { Id = "s2", Email = "s2@test.com", FirstName = "Sub", LastName = "Two", Status = true, DateCreated = now, DateModified = now },
                new MailingListSubscriber { Id = "s3", Email = "s3@test.com", FirstName = "Sub", LastName = "Three", Status = false, DateCreated = now, DateModified = now }
            };

            // Act
            foreach (var subscriber in subscribers)
            {
                _service.Add(subscriber);
            }

            // Assert
            _context.MailingListSubscribers.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingSubscriber_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "update-sub",
                Email = "original@example.com",
                FirstName = "Original",
                LastName = "Name",
                Status = true,
                DateCreated = now,
                DateModified = now
            };
            _context.MailingListSubscribers.Add(subscriber);
            _context.SaveChanges();

            // Act
            subscriber.Email = "updated@example.com";
            subscriber.FirstName = "Updated";
            subscriber.Status = false;
            subscriber.DateModified = now.AddDays(1);
            _service.Update(subscriber);

            // Assert
            var updated = _context.MailingListSubscribers.Find("update-sub");
            updated!.Email.Should().Be("updated@example.com");
            updated.FirstName.Should().Be("Updated");
            updated.Status.Should().BeFalse();
        }

        [Test]
        public void Update_ChangeSubscriberStatus_Updates()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "status-sub",
                Email = "status@example.com",
                FirstName = "Status",
                LastName = "Test",
                Status = true,
                DateCreated = now,
                DateModified = now
            };
            _context.MailingListSubscribers.Add(subscriber);
            _context.SaveChanges();

            // Act
            subscriber.Status = false;
            _service.Update(subscriber);

            // Assert
            var updated = _context.MailingListSubscribers.Find("status-sub");
            updated!.Status.Should().BeFalse();
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "delete-sub",
                Email = "delete@example.com",
                FirstName = "Delete",
                LastName = "Me",
                Status = true,
                DateCreated = now,
                DateModified = now
            };
            _context.MailingListSubscribers.Add(subscriber);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-sub");

            // Assert
            var deleted = _context.MailingListSubscribers.Find("delete-sub");
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
        public void MailingListSubscriber_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "lifecycle-sub",
                Email = "lifecycle@example.com",
                FirstName = "Lifecycle",
                LastName = "Test",
                Status = true,
                DateCreated = now,
                DateModified = now
            };

            // Act - Add
            _service.Add(subscriber);
            var added = _service.Get("lifecycle-sub");
            added.Should().NotBeNull();

            // Act - Update
            added!.Email = "updated-lifecycle@example.com";
            added.Status = false;
            _service.Update(added);
            var updated = _service.Get("lifecycle-sub");
            updated!.Email.Should().Be("updated-lifecycle@example.com");

            // Act - Delete
            _service.Delete("lifecycle-sub");
            var deleted = _service.Get("lifecycle-sub");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageSubscribersList_ActiveAndInactiveSubscribers()
        {
            // Arrange - Simulate managing an active subscriber base
            var now = DateTime.UtcNow;
            var subscribers = new[]
            {
                new MailingListSubscriber { Id = "active-1", Email = "active1@company.com", FirstName = "Active", LastName = "One", Status = true, DateCreated = now, DateModified = now },
                new MailingListSubscriber { Id = "active-2", Email = "active2@company.com", FirstName = "Active", LastName = "Two", Status = true, DateCreated = now, DateModified = now },
                new MailingListSubscriber { Id = "inactive-1", Email = "inactive1@company.com", FirstName = "Inactive", LastName = "One", Status = false, DateCreated = now, DateModified = now }
            };

            // Act
            foreach (var sub in subscribers)
            {
                _service.Add(sub);
            }

            var allSubs = _service.GetAll();
            var activeSubs = allSubs.Where(s => s.Status).ToList();
            var inactiveSubs = allSubs.Where(s => !s.Status).ToList();

            // Assert
            allSubs.Should().HaveCount(3);
            activeSubs.Should().HaveCount(2);
            inactiveSubs.Should().HaveCount(1);
        }

        [Test]
        public void ResubscribePreviouslyUnsubscribedSubscriber()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var subscriber = new MailingListSubscriber
            {
                Id = "reactivate-sub",
                Email = "reactivate@example.com",
                FirstName = "Reactivate",
                LastName = "Test",
                Status = false,
                DateCreated = now,
                DateModified = now
            };
            _service.Add(subscriber);

            // Act - Resubscribe
            var fetched = _service.Get("reactivate-sub");
            fetched!.Status = true;
            fetched.DateModified = now.AddDays(1);
            _service.Update(fetched);

            // Assert
            var reactivated = _service.Get("reactivate-sub");
            reactivated!.Status.Should().BeTrue();
        }

        #endregion
    }
}
