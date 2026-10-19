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
    /// Unit tests for MailingListService - Email marketing mailing list management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Marketing")]
    public class MailingListServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private MailingListService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new MailingListService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsMailingList()
        {
            // Arrange
            var mailingList = new MailingList
            {
                Id = "list-1",
                Name = "Newsletter",
                DefaultEmailFrom = "newsletter@example.com",
                DefaultFromName = "Company Newsletter",
                Description = "Monthly company newsletter",
                Address = "123 Business St, City, State 12345"
            };
            _context.MailingLists.Add(mailingList);
            _context.SaveChanges();

            // Act
            var result = _service.Get("list-1");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Newsletter");
            result.DefaultEmailFrom.Should().Be("newsletter@example.com");
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
        public void GetAll_WithMultipleMailingLists_ReturnsAll()
        {
            // Arrange
            _context.MailingLists.AddRange(
                new MailingList
                {
                    Id = "list-1",
                    Name = "Newsletter",
                    DefaultEmailFrom = "newsletter@example.com",
                    DefaultFromName = "Company",
                    Description = "Monthly news",
                    Address = "123 Main St"
                },
                new MailingList
                {
                    Id = "list-2",
                    Name = "Product Updates",
                    DefaultEmailFrom = "updates@example.com",
                    DefaultFromName = "Products",
                    Description = "Product announcements",
                    Address = "456 Oak Ave"
                },
                new MailingList
                {
                    Id = "list-3",
                    Name = "Promotions",
                    DefaultEmailFrom = "promo@example.com",
                    DefaultFromName = "Marketing",
                    Description = "Special offers",
                    Address = "789 Pine Rd"
                }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoMailingLists_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidMailingList_AddsToDatabase()
        {
            // Arrange
            var mailingList = new MailingList
            {
                Id = "new-list",
                Name = "New Mailing List",
                DefaultEmailFrom = "new@example.com",
                DefaultFromName = "New List Owner",
                Description = "A newly created mailing list",
                Address = "999 New St"
            };

            // Act
            _service.Add(mailingList);

            // Assert
            var saved = _context.MailingLists.Find("new-list");
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Mailing List");
        }

        [Test]
        public void Add_MultipleMailingLists_AllAreSaved()
        {
            // Arrange
            var lists = new[]
            {
                new MailingList { Id = "l1", Name = "List 1", DefaultEmailFrom = "l1@test.com", DefaultFromName = "L1", Description = "Desc1", Address = "Addr1" },
                new MailingList { Id = "l2", Name = "List 2", DefaultEmailFrom = "l2@test.com", DefaultFromName = "L2", Description = "Desc2", Address = "Addr2" },
                new MailingList { Id = "l3", Name = "List 3", DefaultEmailFrom = "l3@test.com", DefaultFromName = "L3", Description = "Desc3", Address = "Addr3" }
            };

            // Act
            foreach (var list in lists)
            {
                _service.Add(list);
            }

            // Assert
            _context.MailingLists.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingMailingList_UpdatesInDatabase()
        {
            // Arrange
            var mailingList = new MailingList
            {
                Id = "update-list",
                Name = "Original Name",
                DefaultEmailFrom = "original@example.com",
                DefaultFromName = "Original Owner",
                Description = "Original description",
                Address = "Original Address"
            };
            _context.MailingLists.Add(mailingList);
            _context.SaveChanges();

            // Act
            mailingList.Name = "Updated Name";
            mailingList.Description = "Updated description";
            mailingList.DefaultEmailFrom = "updated@example.com";
            _service.Update(mailingList);

            // Assert
            var updated = _context.MailingLists.Find("update-list");
            updated!.Name.Should().Be("Updated Name");
            updated.Description.Should().Be("Updated description");
            updated.DefaultEmailFrom.Should().Be("updated@example.com");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var mailingList = new MailingList
            {
                Id = "delete-list",
                Name = "To Delete",
                DefaultEmailFrom = "delete@example.com",
                DefaultFromName = "Delete",
                Description = "Will be deleted",
                Address = "Delete Addr"
            };
            _context.MailingLists.Add(mailingList);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-list");

            // Assert
            var deleted = _context.MailingLists.Find("delete-list");
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
        public void MailingList_FullLifecycle()
        {
            // Arrange
            var mailingList = new MailingList
            {
                Id = "lifecycle-list",
                Name = "Lifecycle List",
                DefaultEmailFrom = "lifecycle@example.com",
                DefaultFromName = "Lifecycle Owner",
                Description = "Lifecycle test list",
                Address = "Lifecycle Address"
            };

            // Act - Add
            _service.Add(mailingList);
            var added = _service.Get("lifecycle-list");
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle List";
            added.Description = "Updated description";
            _service.Update(added);
            var updated = _service.Get("lifecycle-list");
            updated!.Name.Should().Be("Updated Lifecycle List");

            // Act - Delete
            _service.Delete("lifecycle-list");
            var deleted = _service.Get("lifecycle-list");
            deleted.Should().BeNull();
        }

        [Test]
        public void MultipleMailingLists_ManageMultipleCampaignChannels()
        {
            // Arrange - Simulate different marketing channels
            var lists = new[]
            {
                new MailingList
                {
                    Id = "newsletter",
                    Name = "Weekly Newsletter",
                    DefaultEmailFrom = "newsletter@company.com",
                    DefaultFromName = "Newsletter Team",
                    Description = "Weekly content digest",
                    Address = "Company HQ"
                },
                new MailingList
                {
                    Id = "promotions",
                    Name = "Promotional Offers",
                    DefaultEmailFrom = "promo@company.com",
                    DefaultFromName = "Sales Team",
                    Description = "Special deals and offers",
                    Address = "Company HQ"
                },
                new MailingList
                {
                    Id = "product-updates",
                    Name = "Product Updates",
                    DefaultEmailFrom = "products@company.com",
                    DefaultFromName = "Product Team",
                    Description = "New product announcements",
                    Address = "Company HQ"
                }
            };

            // Act
            foreach (var list in lists)
            {
                _service.Add(list);
            }

            var allLists = _service.GetAll();
            var getNewsletter = _service.Get("newsletter");
            var getPromo = _service.Get("promotions");

            // Assert
            allLists.Should().HaveCount(3);
            getNewsletter!.Name.Should().Be("Weekly Newsletter");
            getPromo!.Name.Should().Be("Promotional Offers");
        }

        #endregion
    }
}
