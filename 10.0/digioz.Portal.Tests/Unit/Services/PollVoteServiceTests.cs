using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for PollVoteService - User poll voting/participation tracking
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Engagement")]
    public class PollVoteServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PollVoteService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PollVoteService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPollVote()
        {
            // Arrange
            var vote = new PollVote
            {
                Id = "vote-1",
                UserId = "user-123",
                PollAnswerId = "ans-1"
            };
            _context.PollVotes.Add(vote);
            _context.SaveChanges();

            // Act
            var result = _service.Get("vote-1");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-123");
            result.PollAnswerId.Should().Be("ans-1");
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
        public void GetAll_WithMultipleVotes_ReturnsAll()
        {
            // Arrange
            _context.PollVotes.AddRange(
                new PollVote { Id = "v-1", UserId = "user-1", PollAnswerId = "ans-1" },
                new PollVote { Id = "v-2", UserId = "user-2", PollAnswerId = "ans-2" },
                new PollVote { Id = "v-3", UserId = "user-3", PollAnswerId = "ans-1" }
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

        #region CountByAnswerId Tests

        [Test]
        public void CountByAnswerId_WithValidAnswerId_ReturnsCount()
        {
            // Arrange
            _context.PollVotes.AddRange(
                new PollVote { Id = "v-1", UserId = "user-1", PollAnswerId = "ans-popular" },
                new PollVote { Id = "v-2", UserId = "user-2", PollAnswerId = "ans-popular" },
                new PollVote { Id = "v-3", UserId = "user-3", PollAnswerId = "ans-popular" },
                new PollVote { Id = "v-4", UserId = "user-4", PollAnswerId = "ans-other" }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountByAnswerId("ans-popular");

            // Assert
            count.Should().Be(3);
        }

        [Test]
        public void CountByAnswerId_WithNoVotes_ReturnsZero()
        {
            // Act
            var count = _service.CountByAnswerId("ans-empty");

            // Assert
            count.Should().Be(0);
        }

        #endregion

        #region GetByPollAnswerIds Tests

        [Test]
        public void GetByPollAnswerIds_WithValidIds_ReturnsVotes()
        {
            // Arrange
            _context.PollVotes.AddRange(
                new PollVote { Id = "v-1", UserId = "user-1", PollAnswerId = "ans-a" },
                new PollVote { Id = "v-2", UserId = "user-2", PollAnswerId = "ans-a" },
                new PollVote { Id = "v-3", UserId = "user-3", PollAnswerId = "ans-b" },
                new PollVote { Id = "v-4", UserId = "user-4", PollAnswerId = "ans-c" },
                new PollVote { Id = "v-5", UserId = "user-5", PollAnswerId = "ans-d" }
            );
            _context.SaveChanges();

            // Act
            var answerIds = new[] { "ans-a", "ans-b" };
            var results = _service.GetByPollAnswerIds(answerIds);

            // Assert
            results.Should().HaveCount(3);
            results.TrueForAll(v => v.PollAnswerId == "ans-a" || v.PollAnswerId == "ans-b").Should().BeTrue();
        }

        [Test]
        public void GetByPollAnswerIds_WithEmptyList_ReturnsEmpty()
        {
            // Act
            var results = _service.GetByPollAnswerIds(new List<string>());

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidVote_AddsToDatabase()
        {
            // Arrange
            var vote = new PollVote
            {
                Id = "new-vote",
                UserId = "new-user",
                PollAnswerId = "new-ans"
            };

            // Act
            _service.Add(vote);

            // Assert
            var saved = _context.PollVotes.Find("new-vote");
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be("new-user");
        }

        [Test]
        public void Add_MultipleUserVotes_Saves()
        {
            // Arrange
            var votes = new[]
            {
                new PollVote { Id = "v-1", UserId = "alice", PollAnswerId = "ans-yes" },
                new PollVote { Id = "v-2", UserId = "bob", PollAnswerId = "ans-yes" },
                new PollVote { Id = "v-3", UserId = "charlie", PollAnswerId = "ans-no" },
                new PollVote { Id = "v-4", UserId = "diana", PollAnswerId = "ans-maybe" }
            };

            // Act
            foreach (var vote in votes)
            {
                _service.Add(vote);
            }

            // Assert
            _context.PollVotes.Should().HaveCount(4);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingVote_UpdatesInDatabase()
        {
            // Arrange
            var vote = new PollVote
            {
                Id = "vote-change",
                UserId = "user-x",
                PollAnswerId = "ans-1"
            };
            _context.PollVotes.Add(vote);
            _context.SaveChanges();

            // Act
            vote.PollAnswerId = "ans-2";
            _service.Update(vote);

            // Assert
            var updated = _context.PollVotes.Find("vote-change");
            updated!.PollAnswerId.Should().Be("ans-2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var vote = new PollVote
            {
                Id = "vote-delete",
                UserId = "temp-user",
                PollAnswerId = "temp-ans"
            };
            _context.PollVotes.Add(vote);
            _context.SaveChanges();

            // Act
            _service.Delete("vote-delete");

            // Assert
            var deleted = _context.PollVotes.Find("vote-delete");
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

        #region DeleteByAnswerId Tests

        [Test]
        public void DeleteByAnswerId_WithValidAnswerId_RemovesAllVotes()
        {
            // Arrange
            _context.PollVotes.AddRange(
                new PollVote { Id = "v-1", UserId = "u-1", PollAnswerId = "ans-to-delete" },
                new PollVote { Id = "v-2", UserId = "u-2", PollAnswerId = "ans-to-delete" },
                new PollVote { Id = "v-3", UserId = "u-3", PollAnswerId = "ans-keep" }
            );
            _context.SaveChanges();

            // Act
            _service.DeleteByAnswerId("ans-to-delete");

            // Assert
            var remaining = _context.PollVotes.Where(v => v.PollAnswerId == "ans-to-delete").ToList();
            remaining.Should().BeEmpty();

            var kept = _context.PollVotes.Find("v-3");
            kept.Should().NotBeNull();
        }

        #endregion

        #region DeleteByPollId Tests

        [Test]
        public void DeleteByPollId_WithValidPollIdAndAnswers_RemovesAllVotes()
        {
            // Arrange
            _context.PollVotes.AddRange(
                new PollVote { Id = "v-1", UserId = "u-1", PollAnswerId = "poll1-ans1" },
                new PollVote { Id = "v-2", UserId = "u-2", PollAnswerId = "poll1-ans1" },
                new PollVote { Id = "v-3", UserId = "u-3", PollAnswerId = "poll1-ans2" },
                new PollVote { Id = "v-4", UserId = "u-4", PollAnswerId = "poll2-ans1" }
            );
            _context.SaveChanges();

            // Act
            var poll1Answers = new[] { "poll1-ans1", "poll1-ans2" };
            _service.DeleteByPollId("poll1", poll1Answers);

            // Assert
            var remaining = _context.PollVotes.Where(v => v.PollAnswerId == "poll1-ans1" || v.PollAnswerId == "poll1-ans2").ToList();
            remaining.Should().BeEmpty();

            var poll2Vote = _context.PollVotes.Find("v-4");
            poll2Vote.Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PollVote_FullLifecycle()
        {
            // Arrange
            var vote = new PollVote
            {
                Id = "lifecycle-vote",
                UserId = "lifecycle-user",
                PollAnswerId = "lifecycle-ans"
            };

            // Act - Add
            _service.Add(vote);
            var added = _service.Get("lifecycle-vote");
            added.Should().NotBeNull();

            // Act - Update
            added!.PollAnswerId = "updated-ans";
            _service.Update(added);
            var updated = _service.Get("lifecycle-vote");
            updated!.PollAnswerId.Should().Be("updated-ans");

            // Act - Delete
            _service.Delete("lifecycle-vote");
            var deleted = _service.Get("lifecycle-vote");
            deleted.Should().BeNull();
        }

        [Test]
        public void ConductPoll_WithMultipleUsers()
        {
            // Arrange - Simulate a poll with users voting
            var answerIds = new[] { "ans-yes", "ans-no", "ans-maybe" };
            var users = new[] { "user-1", "user-2", "user-3", "user-4", "user-5" };

            var votes = new[]
            {
                new PollVote { Id = "v-1", UserId = "user-1", PollAnswerId = "ans-yes" },
                new PollVote { Id = "v-2", UserId = "user-2", PollAnswerId = "ans-yes" },
                new PollVote { Id = "v-3", UserId = "user-3", PollAnswerId = "ans-yes" },
                new PollVote { Id = "v-4", UserId = "user-4", PollAnswerId = "ans-no" },
                new PollVote { Id = "v-5", UserId = "user-5", PollAnswerId = "ans-maybe" }
            };

            // Act
            foreach (var vote in votes)
            {
                _service.Add(vote);
            }

            var yesCount = _service.CountByAnswerId("ans-yes");
            var noCount = _service.CountByAnswerId("ans-no");
            var maybeCount = _service.CountByAnswerId("ans-maybe");
            var allVotes = _service.GetAll();

            // Assert
            allVotes.Should().HaveCount(5);
            yesCount.Should().Be(3);
            noCount.Should().Be(1);
            maybeCount.Should().Be(1);
        }

        [Test]
        public void PollResults_ByAnswer()
        {
            // Arrange - Track votes by answer option
            var pollAnswers = new[] { "rating-excellent", "rating-good", "rating-average", "rating-poor" };

            _context.PollVotes.AddRange(
                new PollVote { Id = "r-1", UserId = "u-1", PollAnswerId = "rating-excellent" },
                new PollVote { Id = "r-2", UserId = "u-2", PollAnswerId = "rating-excellent" },
                new PollVote { Id = "r-3", UserId = "u-3", PollAnswerId = "rating-excellent" },
                new PollVote { Id = "r-4", UserId = "u-4", PollAnswerId = "rating-good" },
                new PollVote { Id = "r-5", UserId = "u-5", PollAnswerId = "rating-good" },
                new PollVote { Id = "r-6", UserId = "u-6", PollAnswerId = "rating-average" }
            );
            _context.SaveChanges();

            // Act
            var results = new Dictionary<string, int>();
            foreach (var answer in pollAnswers)
            {
                results[answer] = _service.CountByAnswerId(answer);
            }

            // Assert
            results["rating-excellent"].Should().Be(3);
            results["rating-good"].Should().Be(2);
            results["rating-average"].Should().Be(1);
            results["rating-poor"].Should().Be(0);
        }

        [Test]
        public void RecalculatePollVotes_DeleteAndRecreate()
        {
            // Arrange - Initial votes
            _context.PollVotes.AddRange(
                new PollVote { Id = "v-1", UserId = "u-1", PollAnswerId = "ans-1" },
                new PollVote { Id = "v-2", UserId = "u-2", PollAnswerId = "ans-2" }
            );
            _context.SaveChanges();

            // Act - Delete votes for this poll
            var answerIds = new[] { "ans-1", "ans-2" };
            _service.DeleteByPollId("poll-x", answerIds);

            // Act - Recalculate with new distribution
            var newVotes = new[]
            {
                new PollVote { Id = "new-1", UserId = "u-1", PollAnswerId = "ans-1" },
                new PollVote { Id = "new-2", UserId = "u-2", PollAnswerId = "ans-1" },
                new PollVote { Id = "new-3", UserId = "u-3", PollAnswerId = "ans-2" },
                new PollVote { Id = "new-4", UserId = "u-4", PollAnswerId = "ans-2" },
                new PollVote { Id = "new-5", UserId = "u-5", PollAnswerId = "ans-2" }
            };

            foreach (var vote in newVotes)
            {
                _service.Add(vote);
            }

            var final = _service.GetAll();
            var ans1Count = _service.CountByAnswerId("ans-1");
            var ans2Count = _service.CountByAnswerId("ans-2");

            // Assert
            final.Should().HaveCount(5);
            ans1Count.Should().Be(2);
            ans2Count.Should().Be(3);
        }

        #endregion
    }
}
