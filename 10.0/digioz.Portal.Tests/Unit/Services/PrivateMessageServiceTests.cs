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
    /// Unit tests for PrivateMessageService - User-to-user private messaging system
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Communication")]
    public class PrivateMessageServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PrivateMessageService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PrivateMessageService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPrivateMessage()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "user-alice",
                ToId = "user-bob",
                Subject = "Hello",
                Message = "How are you?",
                SentDate = now,
                IsRead = false,
                Reported = false
            };
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.FromId.Should().Be("user-alice");
            result.ToId.Should().Be("user-bob");
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

        #region GetInbox Tests

        [Test]
        public void GetInbox_WithValidUserId_ReturnsInboxMessages()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.PrivateMessages.AddRange(
                new PrivateMessage { Id = 1, FromId = "sender-1", ToId = "bob", Subject = "Msg1", Message = "Content1", SentDate = now, IsRead = false },
                new PrivateMessage { Id = 2, FromId = "sender-2", ToId = "bob", Subject = "Msg2", Message = "Content2", SentDate = now, IsRead = true },
                new PrivateMessage { Id = 3, FromId = "bob", ToId = "recipient", Subject = "Msg3", Message = "Content3", SentDate = now, IsRead = false }
            );
            _context.SaveChanges();

            // Act
            var inbox = _service.GetInbox("bob");

            // Assert
            inbox.Should().HaveCount(2);
            inbox.TrueForAll(m => m.ToId == "bob").Should().BeTrue();
        }

        #endregion

        #region GetSent Tests

        [Test]
        public void GetSent_WithValidUserId_ReturnsSentMessages()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.PrivateMessages.AddRange(
                new PrivateMessage { Id = 1, FromId = "alice", ToId = "bob", Subject = "Msg1", Message = "Content1", SentDate = now, IsRead = true },
                new PrivateMessage { Id = 2, FromId = "alice", ToId = "charlie", Subject = "Msg2", Message = "Content2", SentDate = now, IsRead = true },
                new PrivateMessage { Id = 3, FromId = "bob", ToId = "alice", Subject = "Reply", Message = "Reply content", SentDate = now, IsRead = false }
            );
            _context.SaveChanges();

            // Act
            var sent = _service.GetSent("alice");

            // Assert
            sent.Should().HaveCount(2);
            sent.TrueForAll(m => m.FromId == "alice" && m.IsRead).Should().BeTrue();
        }

        #endregion

        #region GetUnreadCount Tests

        [Test]
        public void GetUnreadCount_WithValidUserId_ReturnsUnreadMessageCount()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.PrivateMessages.AddRange(
                new PrivateMessage { Id = 1, FromId = "sender-1", ToId = "alice", Subject = "Unread1", Message = "Content", SentDate = now, IsRead = false },
                new PrivateMessage { Id = 2, FromId = "sender-2", ToId = "alice", Subject = "Unread2", Message = "Content", SentDate = now, IsRead = false },
                new PrivateMessage { Id = 3, FromId = "sender-3", ToId = "alice", Subject = "Read", Message = "Content", SentDate = now, IsRead = true }
            );
            _context.SaveChanges();

            // Act
            var unreadCount = _service.GetUnreadCount("alice");

            // Assert
            unreadCount.Should().Be(2);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidMessage_AddsToDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "alice",
                ToId = "bob",
                Subject = "Test",
                Message = "Test message",
                SentDate = now,
                IsRead = false,
                Reported = false
            };

            // Act
            _service.Add(message);

            // Assert
            var saved = _context.PrivateMessages.Find(1);
            saved.Should().NotBeNull();
            saved!.Subject.Should().Be("Test");
        }

        #endregion

        #region MarkRead Tests

        [Test]
        public void MarkRead_WithValidId_MarksMessageAsRead()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "sender",
                ToId = "receiver",
                Subject = "Test",
                Message = "Content",
                SentDate = now,
                IsRead = false
            };
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();

            // Act
            _service.MarkRead(1);

            // Assert
            var marked = _context.PrivateMessages.Find(1);
            marked!.IsRead.Should().BeTrue();
        }

        #endregion

        #region MarkReadIfUnread Tests

        [Test]
        public void MarkReadIfUnread_WithUnreadMessage_MarkAsRead()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "sender",
                ToId = "receiver",
                Subject = "Test",
                Message = "Content",
                SentDate = now,
                IsRead = false
            };
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();

            // Act
            _service.MarkReadIfUnread(1);

            // Assert
            var marked = _context.PrivateMessages.Find(1);
            marked!.IsRead.Should().BeTrue();
        }

        [Test]
        public void MarkReadIfUnread_WithAlreadyReadMessage_DoesNotChange()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "sender",
                ToId = "receiver",
                Subject = "Test",
                Message = "Content",
                SentDate = now,
                IsRead = true
            };
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();

            // Act
            _service.MarkReadIfUnread(1);

            // Assert
            var msg = _context.PrivateMessages.Find(1);
            msg!.IsRead.Should().BeTrue();
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithValidIdAndOwner_RemovesMessage()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "alice",
                ToId = "bob",
                Subject = "Delete me",
                Message = "Content",
                SentDate = now
            };
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();

            // Act
            _service.Delete(1, "alice");

            // Assert
            var deleted = _context.PrivateMessages.Find(1);
            deleted.Should().BeNull();
        }

        #endregion

        #region CountByUserId Tests

        [Test]
        public void CountByUserId_WithValidUserId_ReturnsCount()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.PrivateMessages.AddRange(
                new PrivateMessage { Id = 1, FromId = "alice", ToId = "bob", Subject = "M1", Message = "C1", SentDate = now },
                new PrivateMessage { Id = 2, FromId = "alice", ToId = "charlie", Subject = "M2", Message = "C2", SentDate = now },
                new PrivateMessage { Id = 3, FromId = "bob", ToId = "alice", Subject = "M3", Message = "C3", SentDate = now }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("alice");

            // Assert
            count.Should().Be(3);
        }

        #endregion

        #region Report Tests

        [Test]
        public void Report_WithValidId_MarksMessageAsReported()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "sender",
                ToId = "receiver",
                Subject = "Offensive",
                Message = "Inappropriate content",
                SentDate = now,
                Reported = false
            };
            _context.PrivateMessages.Add(message);
            _context.SaveChanges();

            // Act
            _service.Report(1);

            // Assert
            var reported = _context.PrivateMessages.Find(1);
            reported!.Reported.Should().BeTrue();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PrivateMessage_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var message = new PrivateMessage
            {
                Id = 1,
                FromId = "alice",
                ToId = "bob",
                Subject = "Test",
                Message = "Initial message",
                SentDate = now,
                IsRead = false
            };

            // Act - Add
            _service.Add(message);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Mark Read
            _service.MarkRead(1);
            var marked = _service.Get(1);
            marked!.IsRead.Should().BeTrue();

            // Act - Delete
            _service.Delete(1, "alice");
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ConversationThread_WithMultipleMessages()
        {
            // Arrange - Simulate back-and-forth messages
            var now = DateTime.UtcNow;
            var messages = new[]
            {
                new PrivateMessage { Id = 1, ParentId = null, FromId = "alice", ToId = "bob", Subject = "Question", Message = "msg1", SentDate = now.AddSeconds(-2), IsRead = false },
                new PrivateMessage { Id = 2, ParentId = 1, FromId = "bob", ToId = "alice", Subject = "Question", Message = "msg2", SentDate = now.AddSeconds(-1), IsRead = false },
                new PrivateMessage { Id = 3, ParentId = 2, FromId = "alice", ToId = "bob", Subject = "Question", Message = "msg3", SentDate = now, IsRead = false }
            };

            // Act
            foreach (var msg in messages)
            {
                _service.Add(msg);
            }

            var aliceInbox = _service.GetInbox("alice");
            var bobInbox = _service.GetInbox("bob");
            var aliceUnread = _service.GetUnreadCount("alice");
            var bobUnread = _service.GetUnreadCount("bob");

            // Assert
            aliceInbox.Should().HaveCount(1);
            bobInbox.Should().HaveCount(2);
            aliceUnread.Should().Be(1);
            bobUnread.Should().Be(2);
        }

        #endregion
    }
}
