using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using digioz.Portal.Tests.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for PollService - Critical user engagement feature with voting logic
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Polls")]
    public class PollServiceTests
    {
        private digiozPortalContext _context;
        private PollService _service;

        [SetUp]
        public void Setup()
        {
            _context = TestDataHelper.CreateInMemoryContext();
            _service = new PollService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPoll()
        {
            // Arrange
            var poll = TestDataHelper.CreateTestPoll("poll-1", "user-1");
            _context.Polls.Add(poll);
            _context.SaveChanges();

            // Act
            var result = _service.Get("poll-1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("poll-1");
            result.Slug.Should().Contain("Test poll question");
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
        public void GetAll_WithMultiplePolls_ReturnsAllPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true),
                TestDataHelper.CreateTestPoll("poll-2", "user-2", visible: true, approved: false),
                TestDataHelper.CreateTestPoll("poll-3", "user-3", visible: false, approved: true)
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

        #region GetLatest Tests

        [Test]
        public void GetLatest_ReturnsOnlyVisibleAndApprovedPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true),
                TestDataHelper.CreateTestPoll("poll-2", "user-2", visible: true, approved: false),
                TestDataHelper.CreateTestPoll("poll-3", "user-3", visible: false, approved: true),
                TestDataHelper.CreateTestPoll("poll-4", "user-4", visible: true, approved: true)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetLatest(10);

            // Assert
            results.Should().HaveCount(2);
            results.Should().OnlyContain(p => p.Visible == true && p.Approved == true);
        }

        [Test]
        public void GetLatest_ReturnsRequestedCount()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.Polls.Add(TestDataHelper.CreateTestPoll($"poll-{i}", "user-1", visible: true, approved: true));
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetLatest(5);

            // Assert
            results.Should().HaveCount(5);
        }

        [Test]
        public void GetLatest_OrdersByDateCreatedDescending()
        {
            // Arrange
            var poll1 = TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true);
            poll1.DateCreated = DateTime.UtcNow.AddDays(-3);
            
            var poll2 = TestDataHelper.CreateTestPoll("poll-2", "user-1", visible: true, approved: true);
            poll2.DateCreated = DateTime.UtcNow.AddDays(-1);
            
            var poll3 = TestDataHelper.CreateTestPoll("poll-3", "user-1", visible: true, approved: true);
            poll3.DateCreated = DateTime.UtcNow.AddDays(-2);

            _context.Polls.AddRange(poll1, poll2, poll3);
            _context.SaveChanges();

            // Act
            var results = _service.GetLatest(10);

            // Assert
            results.Should().HaveCount(3);
            results[0].Id.Should().Be("poll-2"); // Most recent
            results[1].Id.Should().Be("poll-3"); // Middle
            results[2].Id.Should().Be("poll-1"); // Oldest
        }

        #endregion

        #region GetLatestFeatured Tests

        [Test]
        public void GetLatestFeatured_ReturnsOnlyFeaturedVisibleAndApprovedPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true, featured: true),
                TestDataHelper.CreateTestPoll("poll-2", "user-2", visible: true, approved: true, featured: false),
                TestDataHelper.CreateTestPoll("poll-3", "user-3", visible: true, approved: false, featured: true),
                TestDataHelper.CreateTestPoll("poll-4", "user-4", visible: true, approved: true, featured: true)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetLatestFeatured(10);

            // Assert
            results.Should().HaveCount(2);
            results.Should().OnlyContain(p => p.Featured && p.Visible == true && p.Approved == true);
        }

        [Test]
        public void GetLatestFeatured_ReturnsRequestedCount()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                _context.Polls.Add(TestDataHelper.CreateTestPoll($"poll-{i}", "user-1", visible: true, approved: true, featured: true));
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetLatestFeatured(3);

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetLatestFeatured_OrdersByDateCreatedDescending()
        {
            // Arrange
            var poll1 = TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true, featured: true);
            poll1.DateCreated = DateTime.UtcNow.AddDays(-2);
            
            var poll2 = TestDataHelper.CreateTestPoll("poll-2", "user-1", visible: true, approved: true, featured: true);
            poll2.DateCreated = DateTime.UtcNow;

            _context.Polls.AddRange(poll1, poll2);
            _context.SaveChanges();

            // Act
            var results = _service.GetLatestFeatured(10);

            // Assert
            results[0].Id.Should().Be("poll-2"); // Most recent
            results[1].Id.Should().Be("poll-1"); // Older
        }

        #endregion

        #region GetByIds Tests

        [Test]
        public void GetByIds_WithValidIds_ReturnsMatchingPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1"),
                TestDataHelper.CreateTestPoll("poll-2", "user-2"),
                TestDataHelper.CreateTestPoll("poll-3", "user-3")
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByIds(new[] { "poll-1", "poll-3" });

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(p => p.Id == "poll-1");
            results.Should().Contain(p => p.Id == "poll-3");
        }

        [Test]
        public void GetByIds_WithEmptyList_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByIds(new List<string>());

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByIds_WithNullList_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByIds(null);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByIds_WithNonExistingIds_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByIds(new[] { "nonexistent-1", "nonexistent-2" });

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetByUserId Tests

        [Test]
        public void GetByUserId_WithExistingUser_ReturnsUserPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1"),
                TestDataHelper.CreateTestPoll("poll-2", "user-1"),
                TestDataHelper.CreateTestPoll("poll-3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-1");

            // Assert
            results.Should().HaveCount(2);
            results.Should().OnlyContain(p => p.UserId == "user-1");
        }

        [Test]
        public void GetByUserId_WithNullUserId_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByUserId(null);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByUserId_WithEmptyUserId_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByUserId(string.Empty);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByUserId_WithNonExistingUser_ReturnsEmptyList()
        {
            // Arrange
            _context.Polls.Add(TestDataHelper.CreateTestPoll("poll-1", "user-1"));
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-999");

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetPaged Tests

        [Test]
        public void GetPaged_ReturnsCorrectPageOfResults()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                var poll = TestDataHelper.CreateTestPoll($"poll-{i}", "user-1");
                poll.DateCreated = DateTime.UtcNow.AddDays(-i);
                _context.Polls.Add(poll);
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetPaged(2, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(10);
            totalCount.Should().Be(25);
        }

        [Test]
        public void GetPaged_OrdersByDateCreatedDescending()
        {
            // Arrange
            var poll1 = TestDataHelper.CreateTestPoll("poll-1", "user-1");
            poll1.DateCreated = DateTime.UtcNow.AddDays(-3);
            
            var poll2 = TestDataHelper.CreateTestPoll("poll-2", "user-1");
            poll2.DateCreated = DateTime.UtcNow.AddDays(-1);

            _context.Polls.AddRange(poll1, poll2);
            _context.SaveChanges();

            // Act
            var results = _service.GetPaged(1, 10, out int totalCount);

            // Assert
            results[0].Id.Should().Be("poll-2"); // Most recent first
            results[1].Id.Should().Be("poll-1");
        }

        [Test]
        public void GetPaged_WithFirstPage_ReturnsFirstResults()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                _context.Polls.Add(TestDataHelper.CreateTestPoll($"poll-{i}", "user-1"));
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetPaged(1, 5, out int totalCount);

            // Assert
            results.Should().HaveCount(5);
            totalCount.Should().Be(15);
        }

        #endregion

        #region GetPagedFiltered Tests

        [Test]
        public void GetPagedFiltered_WithUserId_ReturnsUserPollsAndPublicPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: false, approved: false), // User's private poll
                TestDataHelper.CreateTestPoll("poll-2", "user-1", visible: true, approved: true),   // User's public poll
                TestDataHelper.CreateTestPoll("poll-3", "user-2", visible: true, approved: true),   // Other user's public poll
                TestDataHelper.CreateTestPoll("poll-4", "user-2", visible: false, approved: false)  // Other user's private poll
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetPagedFiltered(1, 10, "user-1", out int totalCount);

            // Assert
            results.Should().HaveCount(3); // User's 2 polls + 1 other public poll
            totalCount.Should().Be(3);
        }

        [Test]
        public void GetPagedFiltered_WithoutUserId_ReturnsOnlyPublicPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true),
                TestDataHelper.CreateTestPoll("poll-2", "user-2", visible: false, approved: true),
                TestDataHelper.CreateTestPoll("poll-3", "user-3", visible: true, approved: false)
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetPagedFiltered(1, 10, null, out int totalCount);

            // Assert
            results.Should().HaveCount(1);
            results.Should().OnlyContain(p => p.Visible == true && p.Approved == true);
        }

        [Test]
        public void GetPagedFiltered_OrdersByDateCreatedDescending()
        {
            // Arrange
            var poll1 = TestDataHelper.CreateTestPoll("poll-1", "user-1", visible: true, approved: true);
            poll1.DateCreated = DateTime.UtcNow.AddDays(-2);
            
            var poll2 = TestDataHelper.CreateTestPoll("poll-2", "user-1", visible: true, approved: true);
            poll2.DateCreated = DateTime.UtcNow;

            _context.Polls.AddRange(poll1, poll2);
            _context.SaveChanges();

            // Act
            var results = _service.GetPagedFiltered(1, 10, "user-1", out int totalCount);

            // Assert
            results[0].Id.Should().Be("poll-2"); // Most recent
            results[1].Id.Should().Be("poll-1"); // Older
        }

        #endregion

        #region CountByUserId Tests

        [Test]
        public void CountByUserId_WithExistingUser_ReturnsCorrectCount()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1"),
                TestDataHelper.CreateTestPoll("poll-2", "user-1"),
                TestDataHelper.CreateTestPoll("poll-3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("user-1");

            // Assert
            count.Should().Be(2);
        }

        [Test]
        public void CountByUserId_WithNullUserId_ReturnsZero()
        {
            // Act
            var count = _service.CountByUserId(null);

            // Assert
            count.Should().Be(0);
        }

        [Test]
        public void CountByUserId_WithEmptyUserId_ReturnsZero()
        {
            // Act
            var count = _service.CountByUserId(string.Empty);

            // Assert
            count.Should().Be(0);
        }

        [Test]
        public void CountByUserId_WithNonExistingUser_ReturnsZero()
        {
            // Arrange
            _context.Polls.Add(TestDataHelper.CreateTestPoll("poll-1", "user-1"));
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("user-999");

            // Assert
            count.Should().Be(0);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidPoll_AddsToDatabase()
        {
            // Arrange
            var poll = TestDataHelper.CreateTestPoll("new-poll", "user-1");

            // Act
            _service.Add(poll);

            // Assert
            var saved = _context.Polls.Find("new-poll");
            saved.Should().NotBeNull();
            saved.UserId.Should().Be("user-1");
        }

        [Test]
        public void Add_WithAllProperties_SavesCorrectly()
        {
            // Arrange
            var poll = TestDataHelper.CreateTestPoll("poll-1", "user-1", 
                visible: true, approved: true, isClosed: false, featured: true);
            poll.AllowMultipleOptionsVote = true;

            // Act
            _service.Add(poll);

            // Assert
            var saved = _context.Polls.Find("poll-1");
            saved.Should().NotBeNull();
            saved.Visible.Should().Be(true);
            saved.Approved.Should().Be(true);
            saved.IsClosed.Should().BeFalse();
            saved.Featured.Should().BeTrue();
            saved.AllowMultipleOptionsVote.Should().BeTrue();
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingPoll_UpdatesInDatabase()
        {
            // Arrange
            var poll = TestDataHelper.CreateTestPoll("poll-1", "user-1", isClosed: false);
            _context.Polls.Add(poll);
            _context.SaveChanges();

            // Act
            poll.IsClosed = true;
            poll.Slug = "Updated poll question?";
            _service.Update(poll);

            // Assert
            var updated = _context.Polls.Find("poll-1");
            updated.IsClosed.Should().BeTrue();
            updated.Slug.Should().Be("Updated poll question?");
        }

        [Test]
        public void Update_ChangesFeaturedStatus_UpdatesCorrectly()
        {
            // Arrange
            var poll = TestDataHelper.CreateTestPoll("poll-1", "user-1", featured: false);
            _context.Polls.Add(poll);
            _context.SaveChanges();

            // Act
            poll.Featured = true;
            _service.Update(poll);

            // Assert
            var updated = _context.Polls.Find("poll-1");
            updated.Featured.Should().BeTrue();
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var poll = TestDataHelper.CreateTestPoll("poll-1", "user-1");
            _context.Polls.Add(poll);
            _context.SaveChanges();

            // Act
            _service.Delete("poll-1");

            // Assert
            var deleted = _context.Polls.Find("poll-1");
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

        #region DeleteByUserId Tests

        [Test]
        public void DeleteByUserId_WithExistingUser_RemovesAllUserPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1"),
                TestDataHelper.CreateTestPoll("poll-2", "user-1"),
                TestDataHelper.CreateTestPoll("poll-3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var deletedCount = _service.DeleteByUserId("user-1");

            // Assert
            deletedCount.Should().Be(2);
            var remaining = _context.Polls.ToList();
            remaining.Should().HaveCount(1);
            remaining[0].UserId.Should().Be("user-2");
        }

        [Test]
        public void DeleteByUserId_WithNullUserId_ReturnsZero()
        {
            // Act
            var deletedCount = _service.DeleteByUserId(null);

            // Assert
            deletedCount.Should().Be(0);
        }

        [Test]
        public void DeleteByUserId_WithEmptyUserId_ReturnsZero()
        {
            // Act
            var deletedCount = _service.DeleteByUserId(string.Empty);

            // Assert
            deletedCount.Should().Be(0);
        }

        [Test]
        public void DeleteByUserId_WithNonExistingUser_ReturnsZero()
        {
            // Arrange
            _context.Polls.Add(TestDataHelper.CreateTestPoll("poll-1", "user-1"));
            _context.SaveChanges();

            // Act
            var deletedCount = _service.DeleteByUserId("user-999");

            // Assert
            deletedCount.Should().Be(0);
        }

        #endregion

        #region ReassignByUserId Tests

        [Test]
        public void ReassignByUserId_WithExistingUser_ReassignsAllPolls()
        {
            // Arrange
            _context.Polls.AddRange(
                TestDataHelper.CreateTestPoll("poll-1", "user-1"),
                TestDataHelper.CreateTestPoll("poll-2", "user-1"),
                TestDataHelper.CreateTestPoll("poll-3", "user-2")
            );
            _context.SaveChanges();

            // Act
            var reassignedCount = _service.ReassignByUserId("user-1", "user-3");

            // Assert
            reassignedCount.Should().Be(2);
            var poll1 = _context.Polls.Find("poll-1");
            var poll2 = _context.Polls.Find("poll-2");
            poll1.UserId.Should().Be("user-3");
            poll2.UserId.Should().Be("user-3");
        }

        [Test]
        public void ReassignByUserId_WithNullFromUserId_ReturnsZero()
        {
            // Act
            var reassignedCount = _service.ReassignByUserId(null, "user-2");

            // Assert
            reassignedCount.Should().Be(0);
        }

        [Test]
        public void ReassignByUserId_WithNullToUserId_ReturnsZero()
        {
            // Act
            var reassignedCount = _service.ReassignByUserId("user-1", null);

            // Assert
            reassignedCount.Should().Be(0);
        }

        [Test]
        public void ReassignByUserId_WithEmptyUserIds_ReturnsZero()
        {
            // Act
            var reassignedCount = _service.ReassignByUserId(string.Empty, string.Empty);

            // Assert
            reassignedCount.Should().Be(0);
        }

        [Test]
        public void ReassignByUserId_WithNonExistingUser_ReturnsZero()
        {
            // Arrange
            _context.Polls.Add(TestDataHelper.CreateTestPoll("poll-1", "user-1"));
            _context.SaveChanges();

            // Act
            var reassignedCount = _service.ReassignByUserId("user-999", "user-2");

            // Assert
            reassignedCount.Should().Be(0);
        }

        #endregion
    }
}
