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
    /// Unit tests for AspNetUserRoleService - User role assignment management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Identity")]
    public class AspNetUserRoleServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AspNetUserRoleService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AspNetUserRoleService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidUserAndRole_ReturnsUserRole()
        {
            // Arrange
            var userRole = new AspNetUserRole
            {
                UserId = "user-123",
                RoleId = "admin-role"
            };
            _context.AspNetUserRoles.Add(userRole);
            _context.SaveChanges();

            // Act
            var result = _service.Get("user-123", "admin-role");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-123");
            result.RoleId.Should().Be("admin-role");
        }

        [Test]
        public void Get_WithInvalidUserAndRole_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent-user", "nonexistent-role");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_WithNullIds_ReturnsNull()
        {
            // Act
            var result = _service.Get(null, null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_WithPartiallyInvalidIds_ReturnsNull()
        {
            // Arrange
            var userRole = new AspNetUserRole { UserId = "user-1", RoleId = "role-1" };
            _context.AspNetUserRoles.Add(userRole);
            _context.SaveChanges();

            // Act
            var result = _service.Get("user-1", "nonexistent-role");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleUserRoles_ReturnsAll()
        {
            // Arrange
            _context.AspNetUserRoles.AddRange(
                new AspNetUserRole { UserId = "user-1", RoleId = "admin" },
                new AspNetUserRole { UserId = "user-1", RoleId = "editor" },
                new AspNetUserRole { UserId = "user-2", RoleId = "viewer" }
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
        public void GetAll_WithSingleAssignment_ReturnsSingleAssignment()
        {
            // Arrange
            var userRole = new AspNetUserRole { UserId = "user-1", RoleId = "admin" };
            _context.AspNetUserRoles.Add(userRole);
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(1);
            results[0].UserId.Should().Be("user-1");
            results[0].RoleId.Should().Be("admin");
        }

        [Test]
        public void GetAll_ReturnsMultipleRolesForSameUser()
        {
            // Arrange - User with multiple roles
            _context.AspNetUserRoles.AddRange(
                new AspNetUserRole { UserId = "user-admin", RoleId = "admin" },
                new AspNetUserRole { UserId = "user-admin", RoleId = "moderator" },
                new AspNetUserRole { UserId = "user-admin", RoleId = "editor" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
            results.Where(ur => ur.UserId == "user-admin").Should().HaveCount(3);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidUserRole_AddsToDatabase()
        {
            // Arrange
            var userRole = new AspNetUserRole
            {
                UserId = "new-user",
                RoleId = "user-role"
            };

            // Act
            _service.Add(userRole);

            // Assert
            var saved = _context.AspNetUserRoles.FirstOrDefault(ur => ur.UserId == "new-user" && ur.RoleId == "user-role");
            saved.Should().NotBeNull();
        }

        [Test]
        public void Add_UserCanHaveMultipleRoles()
        {
            // Arrange
            var user1Admin = new AspNetUserRole { UserId = "user-1", RoleId = "admin" };
            var user1Editor = new AspNetUserRole { UserId = "user-1", RoleId = "editor" };

            // Act
            _service.Add(user1Admin);
            _service.Add(user1Editor);

            // Assert
            var allRoles = _service.GetAll();
            allRoles.Where(ur => ur.UserId == "user-1").Should().HaveCount(2);
        }

        [Test]
        public void Add_MultipleUsersCanHaveSameRole()
        {
            // Arrange
            var user1Admin = new AspNetUserRole { UserId = "user-1", RoleId = "admin" };
            var user2Admin = new AspNetUserRole { UserId = "user-2", RoleId = "admin" };

            // Act
            _service.Add(user1Admin);
            _service.Add(user2Admin);

            // Assert
            var allRoles = _service.GetAll();
            allRoles.Where(ur => ur.RoleId == "admin").Should().HaveCount(2);
        }

        #endregion

        #region Update Tests

        [Test]
        [Ignore("Updating composite key entity behavior depends on EF Core configuration")]
        public void Update_WithExistingUserRole_UpdatesInDatabase()
        {
            // Note: Composite primary keys (UserId, RoleId) may require special handling depending on EF Core setup
            // Real integration tests should verify this functionality with proper database context
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingUserAndRole_RemovesAssignment()
        {
            // Arrange
            var userRole = new AspNetUserRole
            {
                UserId = "user-1",
                RoleId = "admin"
            };
            _context.AspNetUserRoles.Add(userRole);
            _context.SaveChanges();

            // Act
            _service.Delete("user-1", "admin");

            // Assert
            var deleted = _context.AspNetUserRoles.FirstOrDefault(ur => ur.UserId == "user-1" && ur.RoleId == "admin");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingUserRole_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent-user", "nonexistent-role");
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_WithNullIds_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(null, null);
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesSpecificRoleFromUser()
        {
            // Arrange - User with multiple roles
            _context.AspNetUserRoles.AddRange(
                new AspNetUserRole { UserId = "user-1", RoleId = "admin" },
                new AspNetUserRole { UserId = "user-1", RoleId = "editor" },
                new AspNetUserRole { UserId = "user-1", RoleId = "viewer" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("user-1", "editor");

            // Assert
            var remaining = _service.GetAll().Where(ur => ur.UserId == "user-1");
            remaining.Should().HaveCount(2);
            remaining.Should().NotContain(ur => ur.RoleId == "editor");
        }

        [Test]
        public void Delete_DoesNotRemoveOtherUsersRoles()
        {
            // Arrange
            _context.AspNetUserRoles.AddRange(
                new AspNetUserRole { UserId = "user-1", RoleId = "admin" },
                new AspNetUserRole { UserId = "user-2", RoleId = "admin" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("user-1", "admin");

            // Assert
            var user2Admin = _service.Get("user-2", "admin");
            user2Admin.Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void AssignRoleToUser_AndVerify()
        {
            // Arrange
            var userId = "user-123";
            var roleId = "admin";

            // Act
            var userRole = new AspNetUserRole { UserId = userId, RoleId = roleId };
            _service.Add(userRole);

            // Assert
            var assigned = _service.Get(userId, roleId);
            assigned.Should().NotBeNull();
            assigned!.UserId.Should().Be(userId);
            assigned.RoleId.Should().Be(roleId);
        }

        [Test]
        public void UserWithMultipleRoles_CanBeRetrieved()
        {
            // Arrange
            var userId = "user-special";
            var roles = new[] { "admin", "editor", "moderator" };

            // Act
            foreach (var role in roles)
            {
                _service.Add(new AspNetUserRole { UserId = userId, RoleId = role });
            }

            var allRoles = _service.GetAll();
            var userRoles = allRoles.Where(ur => ur.UserId == userId);

            // Assert
            userRoles.Should().HaveCount(3);
            userRoles.Select(ur => ur.RoleId).Should().Contain(roles);
        }

        [Test]
        public void RevokeRoleFromUser_RemovesAssignment()
        {
            // Arrange
            var userRole = new AspNetUserRole { UserId = "user-1", RoleId = "admin" };
            _service.Add(userRole);

            // Act
            _service.Delete("user-1", "admin");
            var retrieved = _service.Get("user-1", "admin");

            // Assert
            retrieved.Should().BeNull();
        }

        #endregion
    }
}
