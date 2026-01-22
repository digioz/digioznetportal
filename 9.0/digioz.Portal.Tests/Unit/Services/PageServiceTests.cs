using NUnit.Framework;
using Moq;
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
    /// Unit tests for PageService - Core content management service
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    public class PageServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PageService _service;

        [SetUp]
        public void Setup()
        {
            // Use In-Memory database for isolated tests
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PageService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPage()
        {
            // Arrange
            var page = new Page
            {
                Id = 1,
                Title = "Test Page",
                Body = "Test Content",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Pages.Add(page);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Page");
            result.Body.Should().Be("Test Content");
            result.Visible.Should().BeTrue();
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
        public void GetAll_WithMultiplePages_ReturnsAllPages()
        {
            // Arrange
            _context.Pages.AddRange(
                new Page { Id = 1, Title = "Page 1", Body = "Content 1", Visible = true },
                new Page { Id = 2, Title = "Page 2", Body = "Content 2", Visible = true },
                new Page { Id = 3, Title = "Page 3", Body = "Content 3", Visible = false }
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

        #region GetByTitle Tests

        [Test]
        public void GetByTitle_WithExistingTitle_ReturnsPage()
        {
            // Arrange
            var page = new Page
            {
                Id = 1,
                Title = "About Us",
                Body = "Company information",
                Visible = true
            };
            _context.Pages.Add(page);
            _context.SaveChanges();

            // Act
            var result = _service.GetByTitle("About Us");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Title.Should().Be("About Us");
        }

        [Test]
        public void GetByTitle_WithNonExistingTitle_ReturnsNull()
        {
            // Act
            var result = _service.GetByTitle("Non-Existing Page");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByUrl Tests

        [Test]
        public void GetByUrl_WithExistingUrl_ReturnsPage()
        {
            // Arrange
            var page = new Page
            {
                Id = 1,
                Title = "Test",
                Url = "test-page",
                Body = "Content",
                Visible = true
            };
            _context.Pages.Add(page);
            _context.SaveChanges();

            // Act
            var result = _service.GetByUrl("test-page");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Url.Should().Be("test-page");
        }

        [Test]
        public void GetByUrl_WithNonExistingUrl_ReturnsNull()
        {
            // Act
            var result = _service.GetByUrl("non-existing");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidPage_AddsPageToDatabase()
        {
            // Arrange
            var page = new Page
            {
                Title = "New Page",
                Body = "New Content",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(page);

            // Assert
            var savedPage = _context.Pages.FirstOrDefault(p => p.Title == "New Page");
            savedPage.Should().NotBeNull();
            savedPage.Body.Should().Be("New Content");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingPage_UpdatesPageInDatabase()
        {
            // Arrange
            var page = new Page
            {
                Id = 1,
                Title = "Original Title",
                Body = "Original Content",
                Visible = true
            };
            _context.Pages.Add(page);
            _context.SaveChanges();

            // Act
            page.Title = "Updated Title";
            page.Body = "Updated Content";
            _service.Update(page);

            // Assert
            var updatedPage = _context.Pages.Find(1);
            updatedPage.Should().NotBeNull();
            updatedPage.Title.Should().Be("Updated Title");
            updatedPage.Body.Should().Be("Updated Content");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesPageFromDatabase()
        {
            // Arrange
            var page = new Page
            {
                Id = 1,
                Title = "Test Page",
                Body = "Test Content",
                Visible = true
            };
            _context.Pages.Add(page);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deletedPage = _context.Pages.Find(1);
            deletedPage.Should().BeNull();
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
        public void Search_WithMatchingTerm_ReturnsMatchingPages()
        {
            // Arrange
            _context.Pages.AddRange(
                new Page { Id = 1, Title = "About Us", Body = "Company info", Visible = true, Timestamp = DateTime.UtcNow.AddDays(-1) },
                new Page { Id = 2, Title = "Contact", Body = "Contact details", Visible = true, Timestamp = DateTime.UtcNow },
                new Page { Id = 3, Title = "Products", Body = "Product catalog", Visible = false }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("contact", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(1);
            results.First().Title.Should().Be("Contact");
            totalCount.Should().Be(1);
        }

        [Test]
        public void Search_WithEmptyTerm_ReturnsAllVisiblePages()
        {
            // Arrange
            _context.Pages.AddRange(
                new Page { Id = 1, Title = "Page 1", Body = "Content 1", Visible = true },
                new Page { Id = 2, Title = "Page 2", Body = "Content 2", Visible = true },
                new Page { Id = 3, Title = "Page 3", Body = "Content 3", Visible = false }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Test]
        public void Search_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                _context.Pages.Add(new Page
                {
                    Id = i,
                    Title = $"Page {i}",
                    Body = $"Content {i}",
                    Visible = true,
                    Timestamp = DateTime.UtcNow.AddDays(-i)
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.Search("", 5, 5, out int totalCount);

            // Assert
            results.Should().HaveCount(5);
            totalCount.Should().Be(15);
        }

        [Test]
        public void Search_WithNonMatchingTerm_ReturnsEmptyList()
        {
            // Arrange
            _context.Pages.Add(new Page
            {
                Id = 1,
                Title = "Test Page",
                Body = "Test Content",
                Visible = true
            });
            _context.SaveChanges();

            // Act
            var results = _service.Search("nonexistent", 0, 10, out int totalCount);

            // Assert
            results.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        #endregion
    }
}
