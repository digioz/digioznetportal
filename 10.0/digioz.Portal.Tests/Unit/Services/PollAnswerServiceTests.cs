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
    /// Unit tests for PollAnswerService - Poll answer/option management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Engagement")]
    public class PollAnswerServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PollAnswerService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PollAnswerService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPollAnswer()
        {
            // Arrange
            var answer = new PollAnswer
            {
                Id = "ans-1",
                PollId = "poll-1",
                Answer = "Yes"
            };
            _context.PollAnswers.Add(answer);
            _context.SaveChanges();

            // Act
            var result = _service.Get("ans-1");

            // Assert
            result.Should().NotBeNull();
            result!.Answer.Should().Be("Yes");
            result.PollId.Should().Be("poll-1");
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
        public void GetAll_WithMultiplePollAnswers_ReturnsAll()
        {
            // Arrange
            _context.PollAnswers.AddRange(
                new PollAnswer { Id = "ans-1", PollId = "poll-1", Answer = "Option 1" },
                new PollAnswer { Id = "ans-2", PollId = "poll-1", Answer = "Option 2" },
                new PollAnswer { Id = "ans-3", PollId = "poll-2", Answer = "Option A" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoAnswers_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetByPollId Tests

        [Test]
        public void GetByPollId_WithValidPollId_ReturnsAnswers()
        {
            // Arrange
            _context.PollAnswers.AddRange(
                new PollAnswer { Id = "a-1", PollId = "poll-survey", Answer = "Strongly Agree" },
                new PollAnswer { Id = "a-2", PollId = "poll-survey", Answer = "Agree" },
                new PollAnswer { Id = "a-3", PollId = "poll-survey", Answer = "Disagree" },
                new PollAnswer { Id = "a-4", PollId = "other-poll", Answer = "Yes" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByPollId("poll-survey");

            // Assert
            results.Should().HaveCount(3);
            results.TrueForAll(a => a.PollId == "poll-survey").Should().BeTrue();
        }

        [Test]
        public void GetByPollId_WithNoPollId_ReturnsEmpty()
        {
            // Act
            var results = _service.GetByPollId("nonexistent");

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetIdsByPollId Tests

        [Test]
        public void GetIdsByPollId_WithValidPollId_ReturnsIds()
        {
            // Arrange
            _context.PollAnswers.AddRange(
                new PollAnswer { Id = "id-1", PollId = "poll-x", Answer = "A1" },
                new PollAnswer { Id = "id-2", PollId = "poll-x", Answer = "A2" },
                new PollAnswer { Id = "id-3", PollId = "poll-y", Answer = "B1" }
            );
            _context.SaveChanges();

            // Act
            var ids = _service.GetIdsByPollId("poll-x");

            // Assert
            ids.Should().HaveCount(2);
            ids.Should().Contain("id-1");
            ids.Should().Contain("id-2");
            ids.Should().NotContain("id-3");
        }

        [Test]
        public void GetIdsByPollId_WithNoPollId_ReturnsEmpty()
        {
            // Act
            var ids = _service.GetIdsByPollId("empty-poll");

            // Assert
            ids.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidAnswer_AddsToDatabase()
        {
            // Arrange
            var answer = new PollAnswer
            {
                Id = "new-ans",
                PollId = "new-poll",
                Answer = "New Answer"
            };

            // Act
            _service.Add(answer);

            // Assert
            var saved = _context.PollAnswers.Find("new-ans");
            saved.Should().NotBeNull();
            saved!.Answer.Should().Be("New Answer");
        }

        [Test]
        public void Add_MultiplePollAnswersSamePoll_Saves()
        {
            // Arrange
            var answers = new[]
            {
                new PollAnswer { Id = "a-1", PollId = "rating", Answer = "Very Good" },
                new PollAnswer { Id = "a-2", PollId = "rating", Answer = "Good" },
                new PollAnswer { Id = "a-3", PollId = "rating", Answer = "Poor" },
                new PollAnswer { Id = "a-4", PollId = "rating", Answer = "Very Poor" }
            };

            // Act
            foreach (var answer in answers)
            {
                _service.Add(answer);
            }

            // Assert
            _context.PollAnswers.Should().HaveCount(4);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingAnswer_UpdatesInDatabase()
        {
            // Arrange
            var answer = new PollAnswer
            {
                Id = "ans-update",
                PollId = "poll-1",
                Answer = "Original Answer"
            };
            _context.PollAnswers.Add(answer);
            _context.SaveChanges();

            // Act
            answer.Answer = "Updated Answer";
            _service.Update(answer);

            // Assert
            var updated = _context.PollAnswers.Find("ans-update");
            updated!.Answer.Should().Be("Updated Answer");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var answer = new PollAnswer
            {
                Id = "delete-ans",
                PollId = "temp-poll",
                Answer = "Delete Me"
            };
            _context.PollAnswers.Add(answer);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-ans");

            // Assert
            var deleted = _context.PollAnswers.Find("delete-ans");
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

        #region DeleteByPollId Tests

        [Test]
        public void DeleteByPollId_WithValidPollId_RemovesAllAnswers()
        {
            // Arrange
            _context.PollAnswers.AddRange(
                new PollAnswer { Id = "a-1", PollId = "poll-delete", Answer = "Option 1" },
                new PollAnswer { Id = "a-2", PollId = "poll-delete", Answer = "Option 2" },
                new PollAnswer { Id = "a-3", PollId = "other-poll", Answer = "Option A" }
            );
            _context.SaveChanges();

            // Act
            _service.DeleteByPollId("poll-delete");

            // Assert
            var remaining = _context.PollAnswers.Where(a => a.PollId == "poll-delete").ToList();
            remaining.Should().BeEmpty();

            var otherRemains = _context.PollAnswers.Find("a-3");
            otherRemains.Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PollAnswer_FullLifecycle()
        {
            // Arrange
            var answer = new PollAnswer
            {
                Id = "lifecycle-ans",
                PollId = "lifecycle-poll",
                Answer = "Initial Answer"
            };

            // Act - Add
            _service.Add(answer);
            var added = _service.Get("lifecycle-ans");
            added.Should().NotBeNull();

            // Act - Update
            added!.Answer = "Updated Answer";
            _service.Update(added);
            var updated = _service.Get("lifecycle-ans");
            updated!.Answer.Should().Be("Updated Answer");

            // Act - Delete
            _service.Delete("lifecycle-ans");
            var deleted = _service.Get("lifecycle-ans");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManagePoll_WithMultipleAnswers()
        {
            // Arrange - Create a multi-choice poll
            var pollId = "satisfaction";
            var answers = new[]
            {
                new PollAnswer { Id = "s-1", PollId = pollId, Answer = "Very Satisfied" },
                new PollAnswer { Id = "s-2", PollId = pollId, Answer = "Satisfied" },
                new PollAnswer { Id = "s-3", PollId = pollId, Answer = "Neutral" },
                new PollAnswer { Id = "s-4", PollId = pollId, Answer = "Dissatisfied" },
                new PollAnswer { Id = "s-5", PollId = pollId, Answer = "Very Dissatisfied" }
            };

            // Act
            foreach (var answer in answers)
            {
                _service.Add(answer);
            }

            var pollAnswers = _service.GetByPollId(pollId);
            var answerIds = _service.GetIdsByPollId(pollId);

            // Assert
            pollAnswers.Should().HaveCount(5);
            answerIds.Should().HaveCount(5);
            answerIds.Should().Contain("s-1", "s-2", "s-3", "s-4", "s-5");
        }

        [Test]
        public void ReplacePollAnswers_DeleteOldAddNew()
        {
            // Arrange
            var oldAnswers = new[]
            {
                new PollAnswer { Id = "old-1", PollId = "poll-edit", Answer = "Option 1" },
                new PollAnswer { Id = "old-2", PollId = "poll-edit", Answer = "Option 2" }
            };

            foreach (var ans in oldAnswers)
            {
                _service.Add(ans);
            }

            // Act - Delete old answers
            _service.DeleteByPollId("poll-edit");

            // Act - Add new answers
            var newAnswers = new[]
            {
                new PollAnswer { Id = "new-1", PollId = "poll-edit", Answer = "New Option 1" },
                new PollAnswer { Id = "new-2", PollId = "poll-edit", Answer = "New Option 2" },
                new PollAnswer { Id = "new-3", PollId = "poll-edit", Answer = "New Option 3" }
            };

            foreach (var ans in newAnswers)
            {
                _service.Add(ans);
            }

            var final = _service.GetByPollId("poll-edit");

            // Assert
            final.Should().HaveCount(3);
            final.All(a => a.Answer.StartsWith("New")).Should().BeTrue();
        }

        #endregion
    }
}
