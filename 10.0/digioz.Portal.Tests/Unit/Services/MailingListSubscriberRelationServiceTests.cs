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
    /// Unit tests for MailingListSubscriberRelationService - Map subscribers to mailing lists
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Marketing")]
    public class MailingListSubscriberRelationServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private MailingListSubscriberRelationService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new MailingListSubscriberRelationService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsRelation()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "rel-1",
                MailingListId = "list-1",
                MailingListSubscriberId = "subscriber-1"
            };
            _context.MailingListSubscriberRelations.Add(relation);
            _context.SaveChanges();

            // Act
            var result = _service.Get("rel-1");

            // Assert
            result.Should().NotBeNull();
            result!.MailingListId.Should().Be("list-1");
            result.MailingListSubscriberId.Should().Be("subscriber-1");
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
        public void GetAll_WithMultipleRelations_ReturnsAll()
        {
            // Arrange
            _context.MailingListSubscriberRelations.AddRange(
                new MailingListSubscriberRelation { Id = "rel-1", MailingListId = "list-1", MailingListSubscriberId = "sub-1" },
                new MailingListSubscriberRelation { Id = "rel-2", MailingListId = "list-1", MailingListSubscriberId = "sub-2" },
                new MailingListSubscriberRelation { Id = "rel-3", MailingListId = "list-2", MailingListSubscriberId = "sub-1" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoRelations_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetByMailingListId Tests

        [Test]
        public void GetByMailingListId_WithValidId_ReturnsSubscribers()
        {
            // Arrange
            _context.MailingListSubscriberRelations.AddRange(
                new MailingListSubscriberRelation { Id = "rel-1", MailingListId = "newsletter", MailingListSubscriberId = "sub-1" },
                new MailingListSubscriberRelation { Id = "rel-2", MailingListId = "newsletter", MailingListSubscriberId = "sub-2" },
                new MailingListSubscriberRelation { Id = "rel-3", MailingListId = "promotions", MailingListSubscriberId = "sub-1" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByMailingListId("newsletter");

            // Assert
            results.Should().HaveCount(2);
            results.TrueForAll(r => r.MailingListId == "newsletter").Should().BeTrue();
        }

        #endregion

        #region GetBySubscriberId Tests

        [Test]
        public void GetBySubscriberId_WithValidId_ReturnsLists()
        {
            // Arrange
            _context.MailingListSubscriberRelations.AddRange(
                new MailingListSubscriberRelation { Id = "rel-1", MailingListId = "newsletter", MailingListSubscriberId = "alice" },
                new MailingListSubscriberRelation { Id = "rel-2", MailingListId = "promotions", MailingListSubscriberId = "alice" },
                new MailingListSubscriberRelation { Id = "rel-3", MailingListId = "newsletter", MailingListSubscriberId = "bob" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetBySubscriberId("alice");

            // Assert
            results.Should().HaveCount(2);
            results.TrueForAll(r => r.MailingListSubscriberId == "alice").Should().BeTrue();
        }

        #endregion

        #region GetByMailingListAndSubscriber Tests

        [Test]
        public void GetByMailingListAndSubscriber_WithValidIds_ReturnsRelation()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "rel-1",
                MailingListId = "list-1",
                MailingListSubscriberId = "subscriber-1"
            };
            _context.MailingListSubscriberRelations.Add(relation);
            _context.SaveChanges();

            // Act
            var result = _service.GetByMailingListAndSubscriber("list-1", "subscriber-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("rel-1");
        }

        [Test]
        public void GetByMailingListAndSubscriber_WithInvalidIds_ReturnsNull()
        {
            // Act
            var result = _service.GetByMailingListAndSubscriber("nonexistent-list", "nonexistent-subscriber");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidRelation_AddsToDatabase()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "new-rel",
                MailingListId = "new-list",
                MailingListSubscriberId = "new-subscriber"
            };

            // Act
            _service.Add(relation);

            // Assert
            var saved = _context.MailingListSubscriberRelations.Find("new-rel");
            saved.Should().NotBeNull();
            saved!.MailingListSubscriberId.Should().Be("new-subscriber");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingRelation_UpdatesInDatabase()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "update-rel",
                MailingListId = "list-1",
                MailingListSubscriberId = "subscriber-1"
            };
            _context.MailingListSubscriberRelations.Add(relation);
            _context.SaveChanges();

            // Act
            relation.MailingListSubscriberId = "subscriber-2";
            _service.Update(relation);

            // Assert
            var updated = _context.MailingListSubscriberRelations.Find("update-rel");
            updated!.MailingListSubscriberId.Should().Be("subscriber-2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "delete-rel",
                MailingListId = "list-1",
                MailingListSubscriberId = "subscriber-1"
            };
            _context.MailingListSubscriberRelations.Add(relation);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-rel");

            // Assert
            var deleted = _context.MailingListSubscriberRelations.Find("delete-rel");
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

        #region DeleteByMailingListAndSubscriber Tests

        [Test]
        public void DeleteByMailingListAndSubscriber_WithValidIds_DeletesRelation()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "rel-1",
                MailingListId = "list-1",
                MailingListSubscriberId = "subscriber-1"
            };
            _context.MailingListSubscriberRelations.Add(relation);
            _context.SaveChanges();

            // Act
            _service.DeleteByMailingListAndSubscriber("list-1", "subscriber-1");

            // Assert
            var deleted = _context.MailingListSubscriberRelations.Find("rel-1");
            deleted.Should().BeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void SubscriberToMailingListMapping_FullLifecycle()
        {
            // Arrange
            var relation = new MailingListSubscriberRelation
            {
                Id = "lifecycle-rel",
                MailingListId = "lifecycle-list",
                MailingListSubscriberId = "lifecycle-subscriber"
            };

            // Act - Add
            _service.Add(relation);
            var added = _service.Get("lifecycle-rel");
            added.Should().NotBeNull();

            // Act - Update
            added!.MailingListSubscriberId = "new-subscriber";
            _service.Update(added);
            var updated = _service.Get("lifecycle-rel");
            updated!.MailingListSubscriberId.Should().Be("new-subscriber");

            // Act - Delete
            _service.Delete("lifecycle-rel");
            var deleted = _service.Get("lifecycle-rel");
            deleted.Should().BeNull();
        }

        [Test]
        public void MultipleSubscribers_ToSingleMailingList()
        {
            // Arrange - One mailing list has many subscribers
            var mailingListId = "newsletter";
            var relations = new[]
            {
                new MailingListSubscriberRelation { Id = "rel-1", MailingListId = mailingListId, MailingListSubscriberId = "subscriber-alice" },
                new MailingListSubscriberRelation { Id = "rel-2", MailingListId = mailingListId, MailingListSubscriberId = "subscriber-bob" },
                new MailingListSubscriberRelation { Id = "rel-3", MailingListId = mailingListId, MailingListSubscriberId = "subscriber-charlie" },
                new MailingListSubscriberRelation { Id = "rel-4", MailingListId = mailingListId, MailingListSubscriberId = "subscriber-david" }
            };

            // Act
            foreach (var rel in relations)
            {
                _service.Add(rel);
            }

            var subscribers = _service.GetByMailingListId(mailingListId);

            // Assert
            subscribers.Should().HaveCount(4);
            subscribers.All(s => s.MailingListId == mailingListId).Should().BeTrue();
        }

        [Test]
        public void SingleSubscriber_ToMultipleMailingLists()
        {
            // Arrange - One subscriber is on multiple mailing lists
            var subscriberId = "subscriber-active";
            var relations = new[]
            {
                new MailingListSubscriberRelation { Id = "rel-1", MailingListId = "newsletter", MailingListSubscriberId = subscriberId },
                new MailingListSubscriberRelation { Id = "rel-2", MailingListId = "promotions", MailingListSubscriberId = subscriberId },
                new MailingListSubscriberRelation { Id = "rel-3", MailingListId = "product-updates", MailingListSubscriberId = subscriberId }
            };

            // Act
            foreach (var rel in relations)
            {
                _service.Add(rel);
            }

            var lists = _service.GetBySubscriberId(subscriberId);

            // Assert
            lists.Should().HaveCount(3);
            lists.All(l => l.MailingListSubscriberId == subscriberId).Should().BeTrue();
        }

        [Test]
        public void UnsubscribeFromMailingList()
        {
            // Arrange - Subscribe user to multiple lists
            var subscriberId = "subscriber-to-unsubscribe";
            var mailingListId = "monthly-newsletter";

            var relation = new MailingListSubscriberRelation
            {
                Id = "rel-1",
                MailingListId = mailingListId,
                MailingListSubscriberId = subscriberId
            };
            _service.Add(relation);

            // Act - Verify subscription
            var subscribed = _service.GetByMailingListAndSubscriber(mailingListId, subscriberId);
            subscribed.Should().NotBeNull();

            // Act - Unsubscribe
            _service.DeleteByMailingListAndSubscriber(mailingListId, subscriberId);

            // Assert - Subscription removed
            var unsubscribed = _service.GetByMailingListAndSubscriber(mailingListId, subscriberId);
            unsubscribed.Should().BeNull();
        }

        #endregion
    }
}
