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
    /// Unit tests for MailingListCampaignRelationService - Map campaigns to mailing lists
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Marketing")]
    public class MailingListCampaignRelationServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private MailingListCampaignRelationService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new MailingListCampaignRelationService(_context);
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
            var relation = new MailingListCampaignRelation
            {
                Id = "rel-1",
                MailingListId = "list-1",
                MailingListCampaignId = "campaign-1"
            };
            _context.MailingListCampaignRelations.Add(relation);
            _context.SaveChanges();

            // Act
            var result = _service.Get("rel-1");

            // Assert
            result.Should().NotBeNull();
            result!.MailingListId.Should().Be("list-1");
            result.MailingListCampaignId.Should().Be("campaign-1");
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
            _context.MailingListCampaignRelations.AddRange(
                new MailingListCampaignRelation { Id = "rel-1", MailingListId = "list-1", MailingListCampaignId = "campaign-1" },
                new MailingListCampaignRelation { Id = "rel-2", MailingListId = "list-1", MailingListCampaignId = "campaign-2" },
                new MailingListCampaignRelation { Id = "rel-3", MailingListId = "list-2", MailingListCampaignId = "campaign-1" }
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
        public void GetByMailingListId_WithValidId_ReturnsRelations()
        {
            // Arrange
            _context.MailingListCampaignRelations.AddRange(
                new MailingListCampaignRelation { Id = "rel-1", MailingListId = "list-newsletter", MailingListCampaignId = "campaign-1" },
                new MailingListCampaignRelation { Id = "rel-2", MailingListId = "list-newsletter", MailingListCampaignId = "campaign-2" },
                new MailingListCampaignRelation { Id = "rel-3", MailingListId = "list-promo", MailingListCampaignId = "campaign-1" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByMailingListId("list-newsletter");

            // Assert
            results.Should().HaveCount(2);
            results.TrueForAll(r => r.MailingListId == "list-newsletter").Should().BeTrue();
        }

        #endregion

        #region GetByCampaignId Tests

        [Test]
        public void GetByCampaignId_WithValidId_ReturnsRelations()
        {
            // Arrange
            _context.MailingListCampaignRelations.AddRange(
                new MailingListCampaignRelation { Id = "rel-1", MailingListId = "list-1", MailingListCampaignId = "spring-sale" },
                new MailingListCampaignRelation { Id = "rel-2", MailingListId = "list-2", MailingListCampaignId = "spring-sale" },
                new MailingListCampaignRelation { Id = "rel-3", MailingListId = "list-3", MailingListCampaignId = "summer-sale" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByCampaignId("spring-sale");

            // Assert
            results.Should().HaveCount(2);
            results.TrueForAll(r => r.MailingListCampaignId == "spring-sale").Should().BeTrue();
        }

        #endregion

        #region GetByMailingListAndCampaign Tests

        [Test]
        public void GetByMailingListAndCampaign_WithValidIds_ReturnsRelation()
        {
            // Arrange
            var relation = new MailingListCampaignRelation
            {
                Id = "rel-1",
                MailingListId = "list-1",
                MailingListCampaignId = "campaign-1"
            };
            _context.MailingListCampaignRelations.Add(relation);
            _context.SaveChanges();

            // Act
            var result = _service.GetByMailingListAndCampaign("list-1", "campaign-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("rel-1");
        }

        [Test]
        public void GetByMailingListAndCampaign_WithInvalidIds_ReturnsNull()
        {
            // Act
            var result = _service.GetByMailingListAndCampaign("nonexistent-list", "nonexistent-campaign");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidRelation_AddsToDatabase()
        {
            // Arrange
            var relation = new MailingListCampaignRelation
            {
                Id = "new-rel",
                MailingListId = "new-list",
                MailingListCampaignId = "new-campaign"
            };

            // Act
            _service.Add(relation);

            // Assert
            var saved = _context.MailingListCampaignRelations.Find("new-rel");
            saved.Should().NotBeNull();
            saved!.MailingListId.Should().Be("new-list");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingRelation_UpdatesInDatabase()
        {
            // Arrange
            var relation = new MailingListCampaignRelation
            {
                Id = "update-rel",
                MailingListId = "list-1",
                MailingListCampaignId = "campaign-1"
            };
            _context.MailingListCampaignRelations.Add(relation);
            _context.SaveChanges();

            // Act
            relation.MailingListCampaignId = "campaign-2";
            _service.Update(relation);

            // Assert
            var updated = _context.MailingListCampaignRelations.Find("update-rel");
            updated!.MailingListCampaignId.Should().Be("campaign-2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var relation = new MailingListCampaignRelation
            {
                Id = "delete-rel",
                MailingListId = "list-1",
                MailingListCampaignId = "campaign-1"
            };
            _context.MailingListCampaignRelations.Add(relation);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-rel");

            // Assert
            var deleted = _context.MailingListCampaignRelations.Find("delete-rel");
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

        #region DeleteByMailingListAndCampaign Tests

        [Test]
        public void DeleteByMailingListAndCampaign_WithValidIds_DeletesRelation()
        {
            // Arrange
            var relation = new MailingListCampaignRelation
            {
                Id = "rel-1",
                MailingListId = "list-1",
                MailingListCampaignId = "campaign-1"
            };
            _context.MailingListCampaignRelations.Add(relation);
            _context.SaveChanges();

            // Act
            _service.DeleteByMailingListAndCampaign("list-1", "campaign-1");

            // Assert
            var deleted = _context.MailingListCampaignRelations.Find("rel-1");
            deleted.Should().BeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void CampaignToMailingListMapping_FullLifecycle()
        {
            // Arrange
            var relation = new MailingListCampaignRelation
            {
                Id = "lifecycle-rel",
                MailingListId = "lifecycle-list",
                MailingListCampaignId = "lifecycle-campaign"
            };

            // Act - Add
            _service.Add(relation);
            var added = _service.Get("lifecycle-rel");
            added.Should().NotBeNull();

            // Act - Update
            added!.MailingListCampaignId = "new-campaign";
            _service.Update(added);
            var updated = _service.Get("lifecycle-rel");
            updated!.MailingListCampaignId.Should().Be("new-campaign");

            // Act - Delete
            _service.Delete("lifecycle-rel");
            var deleted = _service.Get("lifecycle-rel");
            deleted.Should().BeNull();
        }

        [Test]
        public void MultipleCampaigns_ToSingleMailingList()
        {
            // Arrange - One mailing list receives multiple campaigns
            var mailingListId = "newsletter";
            var relations = new[]
            {
                new MailingListCampaignRelation { Id = "rel-1", MailingListId = mailingListId, MailingListCampaignId = "campaign-jan" },
                new MailingListCampaignRelation { Id = "rel-2", MailingListId = mailingListId, MailingListCampaignId = "campaign-feb" },
                new MailingListCampaignRelation { Id = "rel-3", MailingListId = mailingListId, MailingListCampaignId = "campaign-mar" }
            };

            // Act
            foreach (var rel in relations)
            {
                _service.Add(rel);
            }

            var campaigns = _service.GetByMailingListId(mailingListId);

            // Assert
            campaigns.Should().HaveCount(3);
            campaigns.All(c => c.MailingListId == mailingListId).Should().BeTrue();
        }

        [Test]
        public void SingleCampaign_ToMultipleMailingLists()
        {
            // Arrange - One campaign sent to multiple mailing lists
            var campaignId = "new-product-launch";
            var relations = new[]
            {
                new MailingListCampaignRelation { Id = "rel-1", MailingListId = "list-premium", MailingListCampaignId = campaignId },
                new MailingListCampaignRelation { Id = "rel-2", MailingListId = "list-standard", MailingListCampaignId = campaignId },
                new MailingListCampaignRelation { Id = "rel-3", MailingListId = "list-vip", MailingListCampaignId = campaignId }
            };

            // Act
            foreach (var rel in relations)
            {
                _service.Add(rel);
            }

            var lists = _service.GetByCampaignId(campaignId);

            // Assert
            lists.Should().HaveCount(3);
            lists.All(l => l.MailingListCampaignId == campaignId).Should().BeTrue();
        }

        #endregion
    }
}
