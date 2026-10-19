using NUnit.Framework;
using Moq;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using digioz.Portal.Tests.Helpers;
using System.Linq;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for CommentService - User comment management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    public class CommentServiceTests
    {
        private digiozPortalContext _context;
        private CommentService _service;

        [SetUp]
        public void Setup()
        {
            _context = TestDataHelper.CreateInMemoryContext();
            _service = new CommentService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsComment()
        {
            // Arrange
            var comment = TestDataHelper.CreateTestComment("1", "user-1");
            _context.Comments.Add(comment);
            _context.SaveChanges();

            // Act
            var result = _service.Get("1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("1");
            result.Body.Should().Be("Test comment");
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
        public void GetAll_WithMultipleComments_ReturnsAllComments()
        {
            // Arrange
            _context.Comments.AddRange(
                TestDataHelper.CreateTestComment("1", "user-1"),
                TestDataHelper.CreateTestComment("2", "user-2"),
                TestDataHelper.CreateTestComment("3", "user-3")
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        #endregion

        #region GetByUserId Tests

        [Test]
        public void GetByUserId_WithExistingUser_ReturnsUserComments()
        {
            // Arrange
            _context.Comments.AddRange(
                TestDataHelper.CreateTestComment("1", "user-1"),
                TestDataHelper.CreateTestComment("2", "user-1"),
                TestDataHelper.CreateTestComment("3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-1");

            // Assert
            results.Should().HaveCount(2);
            results.Should().OnlyContain(c => c.UserId == "user-1");
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidComment_AddsToDatabase()
        {
            // Arrange
            var comment = TestDataHelper.CreateTestComment("new-comment", "user-1");

            // Act
            _service.Add(comment);

            // Assert
            var saved = _context.Comments.Find("new-comment");

            saved.Should().NotBeNull();
            saved!.Should().NotBeNull();
            saved!.Body.Should().Be("Test comment");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingComment_UpdatesInDatabase()
        {
            // Arrange
            var comment = TestDataHelper.CreateTestComment("1", "user-1");
            _context.Comments.Add(comment);
            _context.SaveChanges();

            // Act
            comment.Body = "Updated comment";
            _service.Update(comment);

            // Assert
            var updated = _context.Comments.Find("1");

            updated.Should().NotBeNull();
            updated!.Should().NotBeNull();
            updated!.Body.Should().Be("Updated comment");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var comment = TestDataHelper.CreateTestComment("1", "user-1");
            _context.Comments.Add(comment);
            _context.SaveChanges();

            // Act
            _service.Delete("1");

            // Assert
            var deleted = _context.Comments.Find("1");
            deleted.Should().BeNull();
        }

        #endregion
    }
}
