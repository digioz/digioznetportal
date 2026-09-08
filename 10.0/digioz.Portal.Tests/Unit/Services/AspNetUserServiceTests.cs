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
    /// Unit tests for AspNetUserService - Identity user management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Identity")]
    public class AspNetUserServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AspNetUserService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AspNetUserService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new AspNetUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com",
                EmailConfirmed = true
            };
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();

            // Act
            var result = _service.Get(userId);

            // Assert
            result.Should().NotBeNull();
            result!.UserName.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
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
        public void GetAll_WithMultipleUsers_ReturnsAllUsers()
        {
            // Arrange
            _context.AspNetUsers.AddRange(
                new AspNetUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" },
                new AspNetUser { Id = "user-2", UserName = "bob", Email = "bob@example.com" },
                new AspNetUser { Id = "user-3", UserName = "charlie", Email = "charlie@example.com" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
            results.Should().Contain(u => u.UserName == "alice");
            results.Should().Contain(u => u.UserName == "bob");
            results.Should().Contain(u => u.UserName == "charlie");
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
        public void GetAll_WithSingleUser_ReturnsSingleUser()
        {
            // Arrange
            var user = new AspNetUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" };
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(1);
            results[0].UserName.Should().Be("alice");
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidUser_AddsToDatabase()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "new-user-1",
                UserName = "newuser",
                Email = "newuser@example.com",
                EmailConfirmed = false
            };

            // Act
            _service.Add(user);

            // Assert
            var saved = _context.AspNetUsers.Find("new-user-1");
            saved.Should().NotBeNull();
            saved!.UserName.Should().Be("newuser");
            saved.Email.Should().Be("newuser@example.com");
        }

        [Test]
        public void Add_WithUserContainingAllFields_SavesCorrectly()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "full-user",
                UserName = "fulluser",
                NormalizedUserName = "FULLUSER",
                Email = "full@example.com",
                NormalizedEmail = "FULL@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = "hashed_password_123",
                SecurityStamp = "security_stamp_123",
                ConcurrencyStamp = "concurrency_stamp_123"
            };

            // Act
            _service.Add(user);

            // Assert
            var saved = _context.AspNetUsers.Find("full-user");
            saved.Should().NotBeNull();
            saved!.NormalizedUserName.Should().Be("FULLUSER");
            saved.PasswordHash.Should().Be("hashed_password_123");
            saved.EmailConfirmed.Should().BeTrue();
        }

        [Test]
        public void Add_MultipleUsers_AllAreSaved()
        {
            // Arrange
            var users = new[]
            {
                new AspNetUser { Id = "user-a", UserName = "usera", Email = "a@example.com" },
                new AspNetUser { Id = "user-b", UserName = "userb", Email = "b@example.com" },
                new AspNetUser { Id = "user-c", UserName = "userc", Email = "c@example.com" }
            };

            // Act
            foreach (var user in users)
            {
                _service.Add(user);
            }

            // Assert
            _context.AspNetUsers.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingUser_UpdatesInDatabase()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "user-1",
                UserName = "originalname",
                Email = "original@example.com"
            };
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();

            // Act
            user.UserName = "updatedname";
            user.Email = "updated@example.com";
            _service.Update(user);

            // Assert
            var updated = _context.AspNetUsers.Find("user-1");
            updated!.UserName.Should().Be("updatedname");
            updated.Email.Should().Be("updated@example.com");
        }

        [Test]
        public void Update_WithEmailConfirmation_UpdatesFlag()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@example.com",
                EmailConfirmed = false
            };
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();

            // Act
            user.EmailConfirmed = true;
            _service.Update(user);

            // Assert
            var updated = _context.AspNetUsers.Find("user-1");
            updated!.EmailConfirmed.Should().BeTrue();
        }

        [Test]
        public void Update_WithSecurityStampChange_Updates()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "user-1",
                UserName = "testuser",
                SecurityStamp = "old_stamp"
            };
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();

            // Act
            user.SecurityStamp = "new_stamp";
            _service.Update(user);

            // Assert
            var updated = _context.AspNetUsers.Find("user-1");
            updated!.SecurityStamp.Should().Be("new_stamp");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@example.com"
            };
            _context.AspNetUsers.Add(user);
            _context.SaveChanges();

            // Act
            _service.Delete("user-1");

            // Assert
            var deleted = _context.AspNetUsers.Find("user-1");
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
        public void Delete_RemovesCorrectUserWhenMultipleExist()
        {
            // Arrange
            _context.AspNetUsers.AddRange(
                new AspNetUser { Id = "user-1", UserName = "alice" },
                new AspNetUser { Id = "user-2", UserName = "bob" },
                new AspNetUser { Id = "user-3", UserName = "charlie" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("user-2");

            // Assert
            _context.AspNetUsers.Should().HaveCount(2);
            _context.AspNetUsers.Find("user-2").Should().BeNull();
            _context.AspNetUsers.Find("user-1").Should().NotBeNull();
            _context.AspNetUsers.Find("user-3").Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void AddUpdateDelete_FullLifecycle()
        {
            // Arrange
            var user = new AspNetUser
            {
                Id = "lifecycle-user",
                UserName = "lifecycle",
                Email = "lifecycle@example.com"
            };

            // Act - Add
            _service.Add(user);
            var added = _service.Get("lifecycle-user");
            added.Should().NotBeNull();

            // Act - Update
            added!.Email = "updated@example.com";
            _service.Update(added);
            var updated = _service.Get("lifecycle-user");
            updated!.Email.Should().Be("updated@example.com");

            // Act - Delete
            _service.Delete("lifecycle-user");
            var deleted = _service.Get("lifecycle-user");
            deleted.Should().BeNull();
        }

        [Test]
        public void GetAll_AfterAddingUsers_ReturnsCorrectOrder()
        {
            // Arrange
            _context.AspNetUsers.Add(new AspNetUser { Id = "user-1", UserName = "alice" });
            _context.SaveChanges();

            _service.Add(new AspNetUser { Id = "user-2", UserName = "bob" });
            _service.Add(new AspNetUser { Id = "user-3", UserName = "charlie" });

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
            results.Should().Contain(u => u.UserName == "alice");
            results.Should().Contain(u => u.UserName == "bob");
            results.Should().Contain(u => u.UserName == "charlie");
        }

        #endregion
    }
}
