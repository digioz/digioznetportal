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
    /// Unit tests for MailingListCampaignService - Email campaign management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Marketing")]
    public class MailingListCampaignServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private MailingListCampaignService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new MailingListCampaignService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsCampaign()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "campaign-1",
                Name = "Q1 Newsletter",
                Subject = "Welcome to Q1 Updates",
                FromName = "Newsletter Team",
                FromEmail = "news@example.com",
                Summary = "Q1 summary",
                Body = "<h1>Q1 Content</h1>",
                DateCreated = now
            };
            _context.MailingListCampaigns.Add(campaign);
            _context.SaveChanges();

            // Act
            var result = _service.Get("campaign-1");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Q1 Newsletter");
            result.Subject.Should().Be("Welcome to Q1 Updates");
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
        public void GetAll_WithMultipleCampaigns_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.MailingListCampaigns.AddRange(
                new MailingListCampaign
                {
                    Id = "camp-1",
                    Name = "Spring Sale",
                    Subject = "Special Spring Offers",
                    FromName = "Sales Team",
                    FromEmail = "sales@example.com",
                    Summary = "Spring sale summary",
                    Body = "<h1>Spring Sale</h1>",
                    DateCreated = now
                },
                new MailingListCampaign
                {
                    Id = "camp-2",
                    Name = "Product Launch",
                    Subject = "Introducing Our New Product",
                    FromName = "Product Team",
                    FromEmail = "product@example.com",
                    Summary = "Product launch summary",
                    Body = "<h1>New Product</h1>",
                    DateCreated = now
                },
                new MailingListCampaign
                {
                    Id = "camp-3",
                    Name = "Summer Newsletter",
                    Subject = "Summer Updates",
                    FromName = "Newsletter Team",
                    FromEmail = "newsletter@example.com",
                    Summary = "Summer summary",
                    Body = "<h1>Summer Updates</h1>",
                    DateCreated = now
                }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoCampaigns_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidCampaign_AddsToDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "new-campaign",
                Name = "New Campaign",
                Subject = "New Campaign Subject",
                FromName = "Sender Name",
                FromEmail = "sender@example.com",
                Summary = "Campaign summary",
                Body = "<h1>Campaign Body</h1>",
                DateCreated = now
            };

            // Act
            _service.Add(campaign);

            // Assert
            var saved = _context.MailingListCampaigns.Find("new-campaign");
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Campaign");
        }

        [Test]
        public void Add_MultipleCampaigns_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaigns = new[]
            {
                new MailingListCampaign { Id = "c1", Name = "Campaign 1", Subject = "Subject 1", FromName = "From 1", FromEmail = "from1@test.com", Summary = "Desc1", Body = "Body1", DateCreated = now },
                new MailingListCampaign { Id = "c2", Name = "Campaign 2", Subject = "Subject 2", FromName = "From 2", FromEmail = "from2@test.com", Summary = "Desc2", Body = "Body2", DateCreated = now },
                new MailingListCampaign { Id = "c3", Name = "Campaign 3", Subject = "Subject 3", FromName = "From 3", FromEmail = "from3@test.com", Summary = "Desc3", Body = "Body3", DateCreated = now }
            };

            // Act
            foreach (var campaign in campaigns)
            {
                _service.Add(campaign);
            }

            // Assert
            _context.MailingListCampaigns.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingCampaign_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "update-campaign",
                Name = "Original Name",
                Subject = "Original Subject",
                FromName = "Original Sender",
                FromEmail = "original@example.com",
                Summary = "Original summary",
                Body = "<h1>Original</h1>",
                DateCreated = now
            };
            _context.MailingListCampaigns.Add(campaign);
            _context.SaveChanges();

            // Act
            campaign.Name = "Updated Name";
            campaign.Subject = "Updated Subject";
            campaign.FromEmail = "updated@example.com";
            _service.Update(campaign);

            // Assert
            var updated = _context.MailingListCampaigns.Find("update-campaign");
            updated!.Name.Should().Be("Updated Name");
            updated.Subject.Should().Be("Updated Subject");
            updated.FromEmail.Should().Be("updated@example.com");
        }

        [Test]
        public void Update_ChangeCampaignSubject_Updates()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "subject-campaign",
                Name = "Test Campaign",
                Subject = "Original Subject Line",
                FromName = "Sender",
                FromEmail = "sender@example.com",
                Summary = "Summary",
                Body = "<p>Body</p>",
                DateCreated = now
            };
            _context.MailingListCampaigns.Add(campaign);
            _context.SaveChanges();

            // Act
            campaign.Subject = "New Compelling Subject Line";
            _service.Update(campaign);

            // Assert
            var updated = _context.MailingListCampaigns.Find("subject-campaign");
            updated!.Subject.Should().Be("New Compelling Subject Line");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "delete-campaign",
                Name = "To Delete",
                Subject = "Will be deleted",
                FromName = "Sender",
                FromEmail = "delete@example.com",
                Summary = "Summary",
                Body = "<p>Body</p>",
                DateCreated = now
            };
            _context.MailingListCampaigns.Add(campaign);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-campaign");

            // Assert
            var deleted = _context.MailingListCampaigns.Find("delete-campaign");
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
        public void MailingListCampaign_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "lifecycle-campaign",
                Name = "Lifecycle Campaign",
                Subject = "Lifecycle Subject",
                FromName = "Lifecycle Sender",
                FromEmail = "lifecycle@example.com",
                Summary = "Lifecycle summary",
                Body = "<h1>Lifecycle</h1>",
                DateCreated = now
            };

            // Act - Add
            _service.Add(campaign);
            var added = _service.Get("lifecycle-campaign");
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Campaign";
            added.Subject = "Updated Subject";
            _service.Update(added);
            var updated = _service.Get("lifecycle-campaign");
            updated!.Name.Should().Be("Updated Lifecycle Campaign");

            // Act - Delete
            _service.Delete("lifecycle-campaign");
            var deleted = _service.Get("lifecycle-campaign");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageCampaignSchedule_MultipleCampaignsOverTime()
        {
            // Arrange - Simulate a campaign schedule
            var now = DateTime.UtcNow;
            var campaigns = new[]
            {
                new MailingListCampaign
                {
                    Id = "jan-campaign",
                    Name = "January Newsletter",
                    Subject = "January Updates",
                    FromName = "Editorial Team",
                    FromEmail = "newsletter@company.com",
                    Summary = "Jan summary",
                    Body = "<h1>January</h1>",
                    DateCreated = now
                },
                new MailingListCampaign
                {
                    Id = "feb-campaign",
                    Name = "February Newsletter",
                    Subject = "February Updates",
                    FromName = "Editorial Team",
                    FromEmail = "newsletter@company.com",
                    Summary = "Feb summary",
                    Body = "<h1>February</h1>",
                    DateCreated = now
                },
                new MailingListCampaign
                {
                    Id = "mar-campaign",
                    Name = "March Newsletter",
                    Subject = "March Updates",
                    FromName = "Editorial Team",
                    FromEmail = "newsletter@company.com",
                    Summary = "Mar summary",
                    Body = "<h1>March</h1>",
                    DateCreated = now
                }
            };

            // Act
            foreach (var campaign in campaigns)
            {
                _service.Add(campaign);
            }

            var allCampaigns = _service.GetAll();
            var janCampaign = _service.Get("jan-campaign");
            var newsletterCampaigns = allCampaigns.Where(c => c.Name.Contains("Newsletter")).ToList();

            // Assert
            allCampaigns.Should().HaveCount(3);
            janCampaign!.Name.Should().Be("January Newsletter");
            newsletterCampaigns.Should().HaveCount(3);
        }

        [Test]
        public void UpdateCampaignContent_ReuseCampaignTemplate()
        {
            // Arrange - Create a base campaign
            var now = DateTime.UtcNow;
            var campaign = new MailingListCampaign
            {
                Id = "template-campaign",
                Name = "Monthly Template",
                Subject = "Template Subject - January",
                FromName = "Newsletter Team",
                FromEmail = "newsletter@company.com",
                Summary = "Monthly summary",
                Body = "<h1>Monthly</h1>",
                DateCreated = now
            };
            _service.Add(campaign);

            // Act - Reuse template for next month
            var existing = _service.Get("template-campaign");
            existing!.Subject = "Template Subject - February";
            _service.Update(existing);

            // Assert
            var updated = _service.Get("template-campaign");
            updated!.Subject.Should().Be("Template Subject - February");
            updated.Name.Should().Be("Monthly Template");
        }

        #endregion
    }
}
