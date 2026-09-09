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
    /// Unit tests for AspNetRoleService - Identity role management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Identity")]
    public class AspNetRoleServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AspNetRoleService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AspNetRoleService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsRole()
        {
            // Arrange
            var roleId = "admin-role";
            var role = new AspNetRole
            {
                Id = roleId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            };
            _context.AspNetRoles.Add(role);
            _context.SaveChanges();

            // Act
            var result = _service.Get(roleId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Administrator");
            result.NormalizedName.Should().Be("ADMINISTRATOR");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent-role");

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
        public void GetAll_WithMultipleRoles_ReturnsAllRoles()
        {
            // Arrange
            _context.AspNetRoles.AddRange(
                new AspNetRole { Id = "role-1", Name = "Admin", NormalizedName = "ADMIN" },
                new AspNetRole { Id = "role-2", Name = "User", NormalizedName = "USER" },
                new AspNetRole { Id = "role-3", Name = "Moderator", NormalizedName = "MODERATOR" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
            results.Should().Contain(r => r.Name == "Admin");
            results.Should().Contain(r => r.Name == "User");
            results.Should().Contain(r => r.Name == "Moderator");
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
        public void GetAll_WithSingleRole_ReturnsSingleRole()
        {
            // Arrange
            var role = new AspNetRole { Id = "role-1", Name = "Guest", NormalizedName = "GUEST" };
            _context.AspNetRoles.Add(role);
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Guest");
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidRole_AddsToDatabase()
        {
            // Arrange
            var role = new AspNetRole
            {
                Id = "new-role",
                Name = "Editor",
                NormalizedName = "EDITOR"
            };

            // Act
            _service.Add(role);

            // Assert
            var saved = _context.AspNetRoles.Find("new-role");
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Editor");
            saved.NormalizedName.Should().Be("EDITOR");
        }

        [Test]
        public void Add_WithRoleContainingConcurrencyStamp_SavesCorrectly()
        {
            // Arrange
            var role = new AspNetRole
            {
                Id = "stamped-role",
                Name = "Contributor",
                NormalizedName = "CONTRIBUTOR",
                ConcurrencyStamp = "concurrency_stamp_123"
            };

            // Act
            _service.Add(role);

            // Assert
            var saved = _context.AspNetRoles.Find("stamped-role");
            saved.Should().NotBeNull();
            saved!.ConcurrencyStamp.Should().Be("concurrency_stamp_123");
        }

        [Test]
        public void Add_MultipleRoles_AllAreSaved()
        {
            // Arrange
            var roles = new[]
            {
                new AspNetRole { Id = "role-a", Name = "Admin", NormalizedName = "ADMIN" },
                new AspNetRole { Id = "role-b", Name = "Editor", NormalizedName = "EDITOR" },
                new AspNetRole { Id = "role-c", Name = "Viewer", NormalizedName = "VIEWER" }
            };

            // Act
            foreach (var role in roles)
            {
                _service.Add(role);
            }

            // Assert
            _context.AspNetRoles.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingRole_UpdatesInDatabase()
        {
            // Arrange
            var role = new AspNetRole
            {
                Id = "role-1",
                Name = "OldName",
                NormalizedName = "OLDNAME"
            };
            _context.AspNetRoles.Add(role);
            _context.SaveChanges();

            // Act
            role.Name = "NewName";
            role.NormalizedName = "NEWNAME";
            _service.Update(role);

            // Assert
            var updated = _context.AspNetRoles.Find("role-1");
            updated!.Name.Should().Be("NewName");
            updated.NormalizedName.Should().Be("NEWNAME");
        }

        [Test]
        public void Update_WithConcurrencyStampChange_Updates()
        {
            // Arrange
            var role = new AspNetRole
            {
                Id = "role-1",
                Name = "Admin",
                ConcurrencyStamp = "old_stamp"
            };
            _context.AspNetRoles.Add(role);
            _context.SaveChanges();

            // Act
            role.ConcurrencyStamp = "new_stamp";
            _service.Update(role);

            // Assert
            var updated = _context.AspNetRoles.Find("role-1");
            updated!.ConcurrencyStamp.Should().Be("new_stamp");
        }

        [Test]
        public void Update_DoesNotAffectOtherRoles()
        {
            // Arrange
            _context.AspNetRoles.AddRange(
                new AspNetRole { Id = "role-1", Name = "Admin" },
                new AspNetRole { Id = "role-2", Name = "User" }
            );
            _context.SaveChanges();

            var role1 = _context.AspNetRoles.Find("role-1");
            role1!.Name = "SuperAdmin";

            // Act
            _service.Update(role1);

            // Assert
            _context.AspNetRoles.Find("role-2")!.Name.Should().Be("User");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var role = new AspNetRole
            {
                Id = "role-1",
                Name = "Temporary",
                NormalizedName = "TEMPORARY"
            };
            _context.AspNetRoles.Add(role);
            _context.SaveChanges();

            // Act
            _service.Delete("role-1");

            // Assert
            var deleted = _context.AspNetRoles.Find("role-1");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent-role");
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
        public void Delete_RemovesCorrectRoleWhenMultipleExist()
        {
            // Arrange
            _context.AspNetRoles.AddRange(
                new AspNetRole { Id = "role-1", Name = "Admin" },
                new AspNetRole { Id = "role-2", Name = "User" },
                new AspNetRole { Id = "role-3", Name = "Guest" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("role-2");

            // Assert
            _context.AspNetRoles.Should().HaveCount(2);
            _context.AspNetRoles.Find("role-2").Should().BeNull();
            _context.AspNetRoles.Find("role-1").Should().NotBeNull();
            _context.AspNetRoles.Find("role-3").Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void AddUpdateDelete_FullLifecycle()
        {
            // Arrange
            var role = new AspNetRole
            {
                Id = "lifecycle-role",
                Name = "Lifecycle",
                NormalizedName = "LIFECYCLE"
            };

            // Act - Add
            _service.Add(role);
            var added = _service.Get("lifecycle-role");
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "UpdatedLifecycle";
            _service.Update(added);
            var updated = _service.Get("lifecycle-role");
            updated!.Name.Should().Be("UpdatedLifecycle");

            // Act - Delete
            _service.Delete("lifecycle-role");
            var deleted = _service.Get("lifecycle-role");
            deleted.Should().BeNull();
        }

        [Test]
        public void DefaultRoles_CanBeCreatedAndRetrieved()
        {
            // Arrange - Create standard application roles
            var roles = new[]
            {
                new AspNetRole { Id = "admin-id", Name = "Admin", NormalizedName = "ADMIN" },
                new AspNetRole { Id = "user-id", Name = "User", NormalizedName = "USER" },
                new AspNetRole { Id = "guest-id", Name = "Guest", NormalizedName = "GUEST" }
            };

            // Act
            foreach (var role in roles)
            {
                _service.Add(role);
            }

            var allRoles = _service.GetAll();

            // Assert
            allRoles.Should().HaveCount(3);
            allRoles.Should().Contain(r => r.Name == "Admin");
            allRoles.Should().Contain(r => r.Name == "User");
            allRoles.Should().Contain(r => r.Name == "Guest");
        }

        #endregion
    }
}
