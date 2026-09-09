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
    /// Unit tests for CommentLikeService - Comment like/reaction management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Comments")]
    public class CommentLikeServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private CommentLikeService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new CommentLikeService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsCommentLike()
        {
            // Arrange
            var commentLike = new CommentLike
            {
                Id = "like-1",
                UserId = "user-1",
                CommentId = "comment-1"
            };
            _context.CommentLikes.Add(commentLike);
            _context.SaveChanges();

            // Act
            var result = _service.Get("like-1");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-1");
            result.CommentId.Should().Be("comment-1");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent-id");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_WithNullId_ReturnsNull()
        {
            // Act
            var result = _service.Get(null);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleCommentLikes_ReturnsAll()
        {
            // Arrange
            _context.CommentLikes.AddRange(
                new CommentLike { Id = "1", UserId = "user-1", CommentId = "comment-1" },
                new CommentLike { Id = "2", UserId = "user-2", CommentId = "comment-1" },
                new CommentLike { Id = "3", UserId = "user-1", CommentId = "comment-2" }
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

        [Test]
        public void GetAll_DeliveredInConsistentOrder()
        {
            // Arrange
            _context.CommentLikes.AddRange(
                new CommentLike { Id = "like-1", UserId = "user-1", CommentId = "comment-1" },
                new CommentLike { Id = "like-2", UserId = "user-2", CommentId = "comment-1" },
                new CommentLike { Id = "like-3", UserId = "user-3", CommentId = "comment-1" }
            );
            _context.SaveChanges();

            // Act
            var results1 = _service.GetAll();
            var results2 = _service.GetAll();

            // Assert
            results1.Select(l => l.Id).SequenceEqual(results2.Select(l => l.Id)).Should().BeTrue();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidCommentLike_AddsToDatabase()
        {
            // Arrange
            var commentLike = new CommentLike
            {
                Id = "new-like",
                UserId = "user-1",
                CommentId = "comment-1"
            };

            // Act
            _service.Add(commentLike);

            // Assert
            var saved = _context.CommentLikes.Find("new-like");
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be("user-1");
        }

        [Test]
        public void Add_MultipleCommentLikes_AllAreSaved()
        {
            // Arrange
            var likes = new[]
            {
                new CommentLike { Id = "1", UserId = "user-1", CommentId = "comment-1" },
                new CommentLike { Id = "2", UserId = "user-2", CommentId = "comment-1" },
                new CommentLike { Id = "3", UserId = "user-1", CommentId = "comment-2" }
            };

            // Act
            foreach (var like in likes)
            {
                _service.Add(like);
            }

            // Assert
            _context.CommentLikes.Should().HaveCount(3);
        }

        [Test]
        public void Add_WithSameUserAndComment_AllowsDuplicate()
        {
            // Arrange
            var like1 = new CommentLike { Id = "like-1", UserId = "user-1", CommentId = "comment-1" };
            var like2 = new CommentLike { Id = "like-2", UserId = "user-1", CommentId = "comment-1" };

            // Act
            _service.Add(like1);
            _service.Add(like2);

            // Assert
            _context.CommentLikes.Should().HaveCount(2);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingCommentLike_UpdatesInDatabase()
        {
            // Arrange
            var commentLike = new CommentLike
            {
                Id = "like-1",
                UserId = "user-1",
                CommentId = "comment-1"
            };
            _context.CommentLikes.Add(commentLike);
            _context.SaveChanges();

            // Act
            commentLike.UserId = "user-2";
            commentLike.CommentId = "comment-2";
            _service.Update(commentLike);

            // Assert
            var updated = _context.CommentLikes.Find("like-1");
            updated!.UserId.Should().Be("user-2");
            updated.CommentId.Should().Be("comment-2");
        }

        [Test]
        public void Update_PreservesId()
        {
            // Arrange
            var commentLike = new CommentLike
            {
                Id = "like-1",
                UserId = "user-1",
                CommentId = "comment-1"
            };
            _context.CommentLikes.Add(commentLike);
            _context.SaveChanges();

            // Act
            commentLike.UserId = "user-100";
            _service.Update(commentLike);

            // Assert
            var updated = _context.CommentLikes.Find("like-1");
            updated.Should().NotBeNull();
        }

        [Test]
        public void Update_DoesNotAffectOtherLikes()
        {
            // Arrange
            _context.CommentLikes.AddRange(
                new CommentLike { Id = "1", UserId = "user-1", CommentId = "comment-1" },
                new CommentLike { Id = "2", UserId = "user-2", CommentId = "comment-1" }
            );
            _context.SaveChanges();

            var like1 = _context.CommentLikes.Find("1");
            like1!.UserId = "user-999";

            // Act
            _service.Update(like1);

            // Assert
            _context.CommentLikes.Find("2")!.UserId.Should().Be("user-2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var commentLike = new CommentLike
            {
                Id = "like-1",
                UserId = "user-1",
                CommentId = "comment-1"
            };
            _context.CommentLikes.Add(commentLike);
            _context.SaveChanges();

            // Act
            _service.Delete("like-1");

            // Assert
            var deleted = _context.CommentLikes.Find("like-1");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent-id");
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_WithNullId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(null);
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesCorrectLikeWhenMultipleExist()
        {
            // Arrange
            _context.CommentLikes.AddRange(
                new CommentLike { Id = "1", UserId = "user-1", CommentId = "comment-1" },
                new CommentLike { Id = "2", UserId = "user-2", CommentId = "comment-1" },
                new CommentLike { Id = "3", UserId = "user-3", CommentId = "comment-1" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("2");

            // Assert
            _context.CommentLikes.Should().HaveCount(2);
            _context.CommentLikes.Find("2").Should().BeNull();
            _context.CommentLikes.Find("1").Should().NotBeNull();
            _context.CommentLikes.Find("3").Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void CommentLike_FullLifecycle()
        {
            // Arrange
            var like = new CommentLike
            {
                Id = "lifecycle-like",
                UserId = "user-1",
                CommentId = "comment-1"
            };

            // Act - Add
            _service.Add(like);
            var added = _service.Get("lifecycle-like");
            added.Should().NotBeNull();

            // Act - Update
            added!.UserId = "user-2";
            _service.Update(added);
            var updated = _service.Get("lifecycle-like");
            updated!.UserId.Should().Be("user-2");

            // Act - Delete
            _service.Delete("lifecycle-like");
            var deleted = _service.Get("lifecycle-like");
            deleted.Should().BeNull();
        }

        [Test]
        public void UserCanLikeMultipleComments()
        {
            // Arrange
            var userId = "user-1";

            // Act
            _service.Add(new CommentLike { Id = "1", UserId = userId, CommentId = "comment-1" });
            _service.Add(new CommentLike { Id = "2", UserId = userId, CommentId = "comment-2" });
            _service.Add(new CommentLike { Id = "3", UserId = userId, CommentId = "comment-3" });

            var allLikes = _service.GetAll();
            var userLikes = allLikes.Where(l => l.UserId == userId);

            // Assert
            userLikes.Should().HaveCount(3);
        }

        [Test]
        public void CommentCanHaveMultipleLikes()
        {
            // Arrange
            var commentId = "comment-1";

            // Act
            _service.Add(new CommentLike { Id = "1", UserId = "user-1", CommentId = commentId });
            _service.Add(new CommentLike { Id = "2", UserId = "user-2", CommentId = commentId });
            _service.Add(new CommentLike { Id = "3", UserId = "user-3", CommentId = commentId });

            var allLikes = _service.GetAll();
            var commentLikes = allLikes.Where(l => l.CommentId == commentId);

            // Assert
            commentLikes.Should().HaveCount(3);
        }

        [Test]
        public void UnlikeCommentRemovesUserLike()
        {
            // Arrange
            var likeId = "like-to-remove";
            _service.Add(new CommentLike { Id = likeId, UserId = "user-1", CommentId = "comment-1" });
            var allBefore = _service.GetAll();

            // Act
            _service.Delete(likeId);
            var allAfter = _service.GetAll();

            // Assert
            allBefore.Should().HaveCount(1);
            allAfter.Should().BeEmpty();
        }

        #endregion
    }
}
