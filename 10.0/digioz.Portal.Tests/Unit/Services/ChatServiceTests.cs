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
    /// Unit tests for ChatService - Real-time chat and messaging history
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Communication")]
    public class ChatServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ChatService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ChatService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsChat()
        {
            // Arrange
            var chat = new Chat
            {
                Id = 1,
                UserId = "user-123",
                Message = "Hello, how are you?",
                Timestamp = DateTime.UtcNow
            };
            _context.Chats.Add(chat);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-123");
            result.Message.Should().Be("Hello, how are you?");
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
        public void GetAll_WithMultipleChats_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Chats.AddRange(
                new Chat { Id = 1, UserId = "user-1", Message = "Message 1", Timestamp = now },
                new Chat { Id = 2, UserId = "user-2", Message = "Message 2", Timestamp = now },
                new Chat { Id = 3, UserId = "user-1", Message = "Message 3", Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoChats_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetByUserId Tests

        [Test]
        public void GetByUserId_WithValidUserId_ReturnsUserChats()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Chats.AddRange(
                new Chat { Id = 1, UserId = "user-alice", Message = "Alice msg 1", Timestamp = now },
                new Chat { Id = 2, UserId = "user-alice", Message = "Alice msg 2", Timestamp = now },
                new Chat { Id = 3, UserId = "user-bob", Message = "Bob msg 1", Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserId("user-alice");

            // Assert
            results.Should().HaveCount(2);
            results.TrueForAll(c => c.UserId == "user-alice").Should().BeTrue();
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

        #region CountByUserId Tests

        [Test]
        public void CountByUserId_WithValidUserId_ReturnsCount()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Chats.AddRange(
                new Chat { Id = 1, UserId = "user-1", Message = "Msg 1", Timestamp = now },
                new Chat { Id = 2, UserId = "user-1", Message = "Msg 2", Timestamp = now },
                new Chat { Id = 3, UserId = "user-1", Message = "Msg 3", Timestamp = now },
                new Chat { Id = 4, UserId = "user-2", Message = "Other msg", Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("user-1");

            // Assert
            count.Should().Be(3);
        }

        [Test]
        public void CountByUserId_WithNoChats_ReturnsZero()
        {
            // Act
            var count = _service.CountByUserId("nonexistent");

            // Assert
            count.Should().Be(0);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidChat_AddsToDatabase()
        {
            // Arrange
            var chat = new Chat
            {
                Id = 1,
                UserId = "new-user",
                Message = "New chat message",
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(chat);

            // Assert
            var saved = _context.Chats.Find(1);
            saved.Should().NotBeNull();
            saved!.Message.Should().Be("New chat message");
        }

        [Test]
        public void Add_MultipleChats_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var chats = new[]
            {
                new Chat { Id = 1, UserId = "u1", Message = "m1", Timestamp = now },
                new Chat { Id = 2, UserId = "u2", Message = "m2", Timestamp = now },
                new Chat { Id = 3, UserId = "u1", Message = "m3", Timestamp = now }
            };

            // Act
            foreach (var chat in chats)
            {
                _service.Add(chat);
            }

            // Assert
            _context.Chats.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingChat_UpdatesInDatabase()
        {
            // Arrange
            var chat = new Chat
            {
                Id = 1,
                UserId = "user-1",
                Message = "Original message",
                Timestamp = DateTime.UtcNow
            };
            _context.Chats.Add(chat);
            _context.SaveChanges();

            // Act
            chat.Message = "Updated message";
            _service.Update(chat);

            // Assert
            var updated = _context.Chats.Find(1);
            updated!.Message.Should().Be("Updated message");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var chat = new Chat
            {
                Id = 1,
                UserId = "user-1",
                Message = "Delete me",
                Timestamp = DateTime.UtcNow
            };
            _context.Chats.Add(chat);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Chats.Find(1);
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

        #region DeleteByUserId Tests

        [Test]
        [Ignore("EF Core InMemory does not support ExecuteDelete operations")]
        public void DeleteByUserId_WithValidUserId_RemovesAllUserChats()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Chats.AddRange(
                new Chat { Id = 1, UserId = "user-delete", Message = "Msg 1", Timestamp = now },
                new Chat { Id = 2, UserId = "user-delete", Message = "Msg 2", Timestamp = now },
                new Chat { Id = 3, UserId = "user-keep", Message = "Other msg", Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var deleted = _service.DeleteByUserId("user-delete");

            // Assert
            deleted.Should().Be(2);
            var remaining = _context.Chats.Where(c => c.UserId == "user-delete");
            remaining.Should().BeEmpty();
        }

        #endregion

        #region ReassignByUserId Tests

        [Test]
        [Ignore("EF Core InMemory does not support ExecuteUpdate operations")]
        public void ReassignByUserId_WithValidUserIds_ReassignsChats()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.Chats.AddRange(
                new Chat { Id = 1, UserId = "user-old", Message = "Msg 1", Timestamp = now },
                new Chat { Id = 2, UserId = "user-old", Message = "Msg 2", Timestamp = now },
                new Chat { Id = 3, UserId = "user-other", Message = "Other msg", Timestamp = now }
            );
            _context.SaveChanges();

            // Act
            var updated = _service.ReassignByUserId("user-old", "user-new");

            // Assert
            updated.Should().Be(2);
            var oldChats = _context.Chats.Where(c => c.UserId == "user-old");
            oldChats.Should().BeEmpty();
            var newChats = _context.Chats.Where(c => c.UserId == "user-new").ToList();
            newChats.Should().HaveCount(2);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Chat_FullLifecycle()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var chat = new Chat
            {
                Id = 1,
                UserId = "lifecycle-user",
                Message = "Initial message",
                Timestamp = now
            };

            // Act - Add
            _service.Add(chat);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Message = "Updated message";
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Message.Should().Be("Updated message");

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ChatConversation_TrackMultipleUserMessages()
        {
            // Arrange - Simulate a chat conversation
            var now = DateTime.UtcNow;
            var messages = new[]
            {
                new Chat { Id = 1, UserId = "user-alice", Message = "Hello Bob", Timestamp = now.AddSeconds(-3) },
                new Chat { Id = 2, UserId = "user-bob", Message = "Hi Alice", Timestamp = now.AddSeconds(-2) },
                new Chat { Id = 3, UserId = "user-alice", Message = "How are you?", Timestamp = now.AddSeconds(-1) },
                new Chat { Id = 4, UserId = "user-bob", Message = "Great, thanks!", Timestamp = now }
            };

            // Act
            foreach (var msg in messages)
            {
                _service.Add(msg);
            }

            var aliceMessages = _service.GetByUserId("user-alice");
            var bobMessages = _service.GetByUserId("user-bob");
            var aliceCount = _service.CountByUserId("user-alice");
            var bobCount = _service.CountByUserId("user-bob");

            // Assert
            aliceMessages.Should().HaveCount(2);
            bobMessages.Should().HaveCount(2);
            aliceCount.Should().Be(2);
            bobCount.Should().Be(2);
        }

        #endregion
    }
}
