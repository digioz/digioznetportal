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
    /// Unit tests for LinkService - Website link management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    public class LinkServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private LinkService _service;

        [SetUp]
        public void Setup()
        {
            // Use In-Memory database for isolated tests
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new LinkService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsLink()
        {
            // Arrange
            var link = new Link
            {
                Id = 1,
                Name = "Example Site",
                Url = "https://example.com",
                Description = "A great website",
                Visible = true
            };
            _context.Links.Add(link);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Example Site");
            result.Url.Should().Be("https://example.com");
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
        public void GetAll_WithMultipleLinks_ReturnsAllLinks()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Link 1", Url = "https://site1.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Link 2", Url = "https://site2.com", Visible = true, Approved = false },
                new Link { Id = 3, Name = "Link 3", Url = "https://site3.com", Visible = false, Approved = true }
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

        #region GetAllVisible Tests

        [Test]
        public void GetAllVisible_WithVisibleLinks_ReturnsOnlyVisible()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Visible 1", Url = "https://site1.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Visible 2", Url = "https://site2.com", Visible = true, Approved = true },
                new Link { Id = 3, Name = "Hidden", Url = "https://site3.com", Visible = false, Approved = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAllVisible();

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(l => l.Visible.Should().BeTrue());
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidLink_AddsToDatabase()
        {
            // Arrange
            var link = new Link
            {
                Name = "New Link",
                Url = "https://newsite.com",
                Description = "New website",
                Visible = true,
                Approved = false
            };

            // Act
            _service.Add(link);

            // Assert
            var saved = _context.Links.FirstOrDefault(l => l.Name == "New Link");
            saved.Should().NotBeNull();
            saved!.Url.Should().Be("https://newsite.com");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingLink_UpdatesInDatabase()
        {
            // Arrange
            var link = new Link
            {
                Id = 1,
                Name = "Original Name",
                Url = "https://original.com",
                Visible = true
            };
            _context.Links.Add(link);
            _context.SaveChanges();

            // Act
            link.Name = "Updated Name";
            link.Url = "https://updated.com";
            _service.Update(link);

            // Assert
            var updated = _context.Links.Find(1);
            updated!.Name.Should().Be("Updated Name");
            updated.Url.Should().Be("https://updated.com");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var link = new Link
            {
                Id = 1,
                Name = "To Delete",
                Url = "https://delete.com",
                Visible = true
            };
            _context.Links.Add(link);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Links.Find(1);
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

        #region IncrementViews Tests

        [Test]
        [Ignore("IncrementViews uses ExecuteUpdate which is not supported by in-memory database provider")]
        public void IncrementViews_WithExistingLink_IncrementsViewCount()
        {
            // Note: This method uses bulk ExecuteUpdate which isn't supported by in-memory EF Core provider.
            // Real integration tests should verify this functionality with a real database.
        }

        [Test]
        [Ignore("IncrementViews uses ExecuteUpdate which is not supported by in-memory database provider")]
        public void IncrementViews_WithNonExistingId_DoesNotThrowException()
        {
            // Note: This method uses bulk ExecuteUpdate which isn't supported by in-memory EF Core provider.
            // Real integration tests should verify this functionality with a real database.
        }

        [Test]
        [Ignore("IncrementViews uses ExecuteUpdate which is not supported by in-memory database provider")]
        public void IncrementViews_MultipleIncrements_IncrementsCorrectAmount()
        {
            // Note: This method uses bulk ExecuteUpdate which isn't supported by in-memory EF Core provider.
            // Real integration tests should verify this functionality with a real database.
        }

        #endregion

        #region Search Tests

        [Test]
        public void Search_WithMatchingTerm_ReturnsMatchingLinks()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Developer Tools", Url = "https://dev.com", Description = "Tools for development", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Design Resources", Url = "https://design.com", Description = "Resource library", Visible = true, Approved = true },
                new Link { Id = 3, Name = "Dev Blog", Url = "https://blog.dev", Description = "Development blog", Visible = true, Approved = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("Dev", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Test]
        public void Search_WithEmptyTerm_ReturnsAllVisibleLinks()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Link 1", Url = "https://site1.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Link 2", Url = "https://site2.com", Visible = true, Approved = true },
                new Link { Id = 3, Name = "Hidden", Url = "https://site3.com", Visible = false, Approved = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Test]
        public void Search_WithNonMatchingTerm_ReturnsEmptyList()
        {
            // Arrange
            _context.Links.Add(new Link
            {
                Id = 1,
                Name = "Example",
                Url = "https://example.com",
                Visible = true,
                Approved = true
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
                _context.Links.Add(new Link
                {
                    Id = i,
                    Name = $"Link {i}",
                    Url = $"https://site{i}.com",
                    Visible = true,
                    Approved = true
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

        #region AdminSearch Tests

        [Test]
        public void AdminSearch_WithVisibilityFilter_ReturnsMatchingLinks()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Visible 1", Url = "https://site1.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Hidden 1", Url = "https://site2.com", Visible = false, Approved = true },
                new Link { Id = 3, Name = "Visible 2", Url = "https://site3.com", Visible = true, Approved = false }
            );
            _context.SaveChanges();

            // Act
            var results = _service.AdminSearch("", "visible", "all", null, 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(l => l.Visible.Should().BeTrue());
        }

        [Test]
        public void AdminSearch_WithApprovalFilter_ReturnsMatchingLinks()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Approved 1", Url = "https://site1.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Pending", Url = "https://site2.com", Visible = true, Approved = false },
                new Link { Id = 3, Name = "Approved 2", Url = "https://site3.com", Visible = true, Approved = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.AdminSearch("", "all", "approved", null, 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(l => l.Approved.Should().BeTrue());
        }

        [Test]
        public void AdminSearch_WithSearchQuery_ReturnsMatchingLinks()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Python Resources", Url = "https://python.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "JavaScript Guide", Url = "https://js.com", Visible = true, Approved = true },
                new Link { Id = 3, Name = "Python Blog", Url = "https://pyblog.io", Visible = true, Approved = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.AdminSearch("Python", "all", "all", null, 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Test]
        public void AdminSearch_WithMultipleFilters_AppliesAllFilters()
        {
            // Arrange
            _context.Links.AddRange(
                new Link { Id = 1, Name = "Approved Visible Link", Url = "https://site1.com", Visible = true, Approved = true },
                new Link { Id = 2, Name = "Pending Hidden Link", Url = "https://site2.com", Visible = false, Approved = false },
                new Link { Id = 3, Name = "Approved Hidden Link", Url = "https://site3.com", Visible = false, Approved = true }
            );
            _context.SaveChanges();

            // Act
            var results = _service.AdminSearch("", "visible", "approved", null, 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(1);
            results.First().Visible.Should().BeTrue();
            results.First().Approved.Should().BeTrue();
        }

        #endregion
    }
}
