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
    /// Unit tests for PollUsersVoteService - Track which users have voted in polls
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Polls")]
    public class PollUsersVoteServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PollUsersVoteService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PollUsersVoteService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidPollIdAndUserId_ReturnsPollUsersVote()
        {
            // Arrange
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "poll-1",
                UserId = "user-123",
                DateVoted = DateTime.UtcNow.ToString("O")
            };
            _context.PollUsersVotes.Add(vote);
            _context.SaveChanges();

            // Act
            var result = _service.Get("poll-1", "user-123");

            // Assert
            result.Should().NotBeNull();
            result!.PollId.Should().Be("poll-1");
            result.UserId.Should().Be("user-123");
        }

        [Test]
        public void Get_WithInvalidPollIdAndUserId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent-poll", "nonexistent-user");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleVotes_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            _context.PollUsersVotes.AddRange(
                new PollUsersVote { Id = 1, PollId = "poll-1", UserId = "user-1", DateVoted = now },
                new PollUsersVote { Id = 2, PollId = "poll-1", UserId = "user-2", DateVoted = now },
                new PollUsersVote { Id = 3, PollId = "poll-2", UserId = "user-1", DateVoted = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoVotes_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetByUserId Tests

        [Test]
        public void GetByUserId_WithValidUserId_ReturnsUserVotes()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            _context.PollUsersVotes.AddRange(
                new PollUsersVote { Id = 1, PollId = "poll-1", UserId = "user-alice", DateVoted = now },
                new PollUsersVote { Id = 2, PollId = "poll-2", UserId = "user-alice", DateVoted = now },
                new PollUsersVote { Id = 3, PollId = "poll-1", UserId = "user-bob", DateVoted = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-alice");

            // Assert
            results.Should().HaveCount(2);
            results.TrueForAll(v => v.UserId == "user-alice").Should().BeTrue();
        }

        [Test]
        public void GetByUserId_WithNonExistingUser_ReturnsEmpty()
        {
            // Act
            var results = _service.GetByUserId("nonexistent");

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Exists Tests

        [Test]
        public void Exists_WithValidPollAndUser_ReturnsTrue()
        {
            // Arrange
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "poll-check",
                UserId = "user-check",
                DateVoted = DateTime.UtcNow.ToString("O")
            };
            _context.PollUsersVotes.Add(vote);
            _context.SaveChanges();

            // Act
            var exists = _service.Exists("poll-check", "user-check");

            // Assert
            exists.Should().BeTrue();
        }

        [Test]
        public void Exists_WithInvalidPollAndUser_ReturnsFalse()
        {
            // Act
            var exists = _service.Exists("nonexistent-poll", "nonexistent-user");

            // Assert
            exists.Should().BeFalse();
        }

        [Test]
        public void Exists_WithPollButDifferentUser_ReturnsFalse()
        {
            // Arrange
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "poll-1",
                UserId = "user-1",
                DateVoted = DateTime.UtcNow.ToString("O")
            };
            _context.PollUsersVotes.Add(vote);
            _context.SaveChanges();

            // Act
            var exists = _service.Exists("poll-1", "user-different");

            // Assert
            exists.Should().BeFalse();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidVote_AddsToDatabase()
        {
            // Arrange
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "new-poll",
                UserId = "new-user",
                DateVoted = DateTime.UtcNow.ToString("O")
            };

            // Act
            _service.Add(vote);

            // Assert
            var saved = _context.PollUsersVotes.Find(1);
            saved.Should().NotBeNull();
            saved!.PollId.Should().Be("new-poll");
        }

        [Test]
        public void Add_MultipleVotes_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            var votes = new[]
            {
                new PollUsersVote { Id = 1, PollId = "p1", UserId = "u1", DateVoted = now },
                new PollUsersVote { Id = 2, PollId = "p1", UserId = "u2", DateVoted = now },
                new PollUsersVote { Id = 3, PollId = "p2", UserId = "u1", DateVoted = now }
            };

            // Act
            foreach (var vote in votes)
            {
                _service.Add(vote);
            }

            // Assert
            _context.PollUsersVotes.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingVote_UpdatesInDatabase()
        {
            // Arrange
            var oldDate = DateTime.UtcNow.AddDays(-1).ToString("O");
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "poll-update",
                UserId = "user-update",
                DateVoted = oldDate
            };
            _context.PollUsersVotes.Add(vote);
            _context.SaveChanges();

            // Act
            vote.DateVoted = DateTime.UtcNow.ToString("O");
            _service.Update(vote);

            // Assert
            var updated = _context.PollUsersVotes.Find(1);
            updated!.DateVoted.Should().NotBe(oldDate);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithValidPollIdAndUserId_RemovesFromDatabase()
        {
            // Arrange
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "poll-delete",
                UserId = "user-delete",
                DateVoted = DateTime.UtcNow.ToString("O")
            };
            _context.PollUsersVotes.Add(vote);
            _context.SaveChanges();

            // Act
            _service.Delete("poll-delete", "user-delete");

            // Assert
            var deleted = _context.PollUsersVotes.Find(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingPollAndUser_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent", "nonexistent");
            act.Should().NotThrow();
        }

        #endregion

        #region DeleteByPollId Tests

        [Test]
        [Ignore("EF Core InMemory does not support ExecuteDelete operations")]
        public void DeleteByPollId_WithValidPollId_RemovesAllPollVotes()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            _context.PollUsersVotes.AddRange(
                new PollUsersVote { Id = 1, PollId = "poll-to-delete", UserId = "user-1", DateVoted = now },
                new PollUsersVote { Id = 2, PollId = "poll-to-delete", UserId = "user-2", DateVoted = now },
                new PollUsersVote { Id = 3, PollId = "other-poll", UserId = "user-1", DateVoted = now }
            );
            _context.SaveChanges();

            // Act
            _service.DeleteByPollId("poll-to-delete");

            // Assert
            var remaining = _context.PollUsersVotes.Where(v => v.PollId == "poll-to-delete");
            remaining.Should().BeEmpty();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PollUsersVote_TrackUserParticipation()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            var pollId = "popular-poll";
            var votes = new[]
            {
                new PollUsersVote { Id = 1, PollId = pollId, UserId = "user-1", DateVoted = now },
                new PollUsersVote { Id = 2, PollId = pollId, UserId = "user-2", DateVoted = now },
                new PollUsersVote { Id = 3, PollId = pollId, UserId = "user-3", DateVoted = now },
                new PollUsersVote { Id = 4, PollId = pollId, UserId = "user-4", DateVoted = now }
            };

            // Act
            foreach (var vote in votes)
            {
                _service.Add(vote);
            }

            var allVotes = _service.GetAll();
            var pollVotes = allVotes.Where(v => v.PollId == pollId).ToList();
            var pollParticipants = _service.GetByUserId("user-1").Where(v => v.PollId == pollId);

            // Assert
            allVotes.Should().HaveCount(4);
            pollVotes.Should().HaveCount(4);
            pollParticipants.Should().HaveCount(1);
        }

        [Test]
        public void PollUsersVote_PreventDuplicateUserVotes()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            var pollId = "vote-poll";
            var userId = "voter-1";

            var firstVote = new PollUsersVote { Id = 1, PollId = pollId, UserId = userId, DateVoted = now };
            _service.Add(firstVote);

            // Act
            var exists = _service.Exists(pollId, userId);

            // Assert - User has already voted
            exists.Should().BeTrue();

            // Try to add another vote (in real scenario, business logic would prevent this)
            var secondVote = new PollUsersVote { Id = 2, PollId = pollId, UserId = userId, DateVoted = DateTime.UtcNow.ToString("O") };
            _service.Add(secondVote);

            var userVotes = _service.GetByUserId(userId);

            // In this test, we allow it at the DAL level, but the existence check shows proper tracking
            userVotes.Should().HaveCount(2);
        }

        [Test]
        public void PollUsersVote_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow.ToString("O");
            var vote = new PollUsersVote
            {
                Id = 1,
                PollId = "lifecycle-poll",
                UserId = "lifecycle-user",
                DateVoted = now
            };

            // Act - Add
            _service.Add(vote);
            var exists = _service.Exists("lifecycle-poll", "lifecycle-user");
            exists.Should().BeTrue();

            // Act - Verify
            var retrieved = _service.Get("lifecycle-poll", "lifecycle-user");
            retrieved.Should().NotBeNull();

            // Act - Update
            retrieved!.DateVoted = DateTime.UtcNow.ToString("O");
            _service.Update(retrieved);

            // Act - Delete
            _service.Delete("lifecycle-poll", "lifecycle-user");
            var deleted = _service.Get("lifecycle-poll", "lifecycle-user");
            deleted.Should().BeNull();
        }

        #endregion
    }
}
