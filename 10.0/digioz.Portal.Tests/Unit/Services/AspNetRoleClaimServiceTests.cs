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
    /// Unit tests for AspNetRoleClaimService - Role claims management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Identity")]
    public class AspNetRoleClaimServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AspNetRoleClaimService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AspNetRoleClaimService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsClaim()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                Id = 1,
                RoleId = "admin-role",
                ClaimType = "permission",
                ClaimValue = "manage-users"
            };
            _context.AspNetRoleClaims.Add(claim);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.RoleId.Should().Be("admin-role");
            result.ClaimType.Should().Be("permission");
            result.ClaimValue.Should().Be("manage-users");
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
        public void GetAll_WithMultipleClaims_ReturnsAllClaims()
        {
            // Arrange
            _context.AspNetRoleClaims.AddRange(
                new AspNetRoleClaim { Id = 1, RoleId = "admin", ClaimType = "permission", ClaimValue = "manage-users" },
                new AspNetRoleClaim { Id = 2, RoleId = "admin", ClaimType = "permission", ClaimValue = "manage-content" },
                new AspNetRoleClaim { Id = 3, RoleId = "editor", ClaimType = "permission", ClaimValue = "edit-content" }
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
        public void GetAll_WithSingleClaim_ReturnsSingleClaim()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                Id = 1,
                RoleId = "viewer",
                ClaimType = "permission",
                ClaimValue = "view-content"
            };
            _context.AspNetRoleClaims.Add(claim);
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(1);
            results[0].ClaimType.Should().Be("permission");
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidClaim_AddsToDatabase()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                RoleId = "new-role",
                ClaimType = "permission",
                ClaimValue = "new-permission"
            };

            // Act
            _service.Add(claim);

            // Assert
            var saved = _context.AspNetRoleClaims.FirstOrDefault(c => c.RoleId == "new-role" && c.ClaimType == "permission");
            saved.Should().NotBeNull();
            saved!.ClaimValue.Should().Be("new-permission");
        }

        [Test]
        public void Add_MultipleClaimsForSameRole_AllAreSaved()
        {
            // Arrange
            var roleId = "admin";
            var claims = new[]
            {
                new AspNetRoleClaim { RoleId = roleId, ClaimType = "permission", ClaimValue = "manage-users" },
                new AspNetRoleClaim { RoleId = roleId, ClaimType = "permission", ClaimValue = "manage-roles" },
                new AspNetRoleClaim { RoleId = roleId, ClaimType = "permission", ClaimValue = "manage-content" }
            };

            // Act
            foreach (var claim in claims)
            {
                _service.Add(claim);
            }

            // Assert
            var allClaims = _service.GetAll();
            allClaims.Where(c => c.RoleId == roleId).Should().HaveCount(3);
        }

        [Test]
        public void Add_WithDifferentPermissions_AllAreStored()
        {
            // Arrange & Act
            _service.Add(new AspNetRoleClaim { RoleId = "editor", ClaimType = "permission", ClaimValue = "edit-content" });
            _service.Add(new AspNetRoleClaim { RoleId = "editor", ClaimType = "permission", ClaimValue = "publish-content" });
            _service.Add(new AspNetRoleClaim { RoleId = "editor", ClaimType = "permission", ClaimValue = "delete-own-content" });

            // Assert
            var allClaims = _service.GetAll();
            allClaims.Should().HaveCount(3);
            allClaims.Select(c => c.ClaimValue).Should().Contain(new[] { "edit-content", "publish-content", "delete-own-content" });
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingClaim_UpdatesInDatabase()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                Id = 1,
                RoleId = "admin",
                ClaimType = "permission",
                ClaimValue = "old-permission"
            };
            _context.AspNetRoleClaims.Add(claim);
            _context.SaveChanges();

            // Act
            claim.ClaimValue = "new-permission";
            _service.Update(claim);

            // Assert
            var updated = _context.AspNetRoleClaims.Find(1);
            updated!.ClaimValue.Should().Be("new-permission");
        }

        [Test]
        public void Update_WithClaimTypeChange_Updates()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                Id = 1,
                RoleId = "admin",
                ClaimType = "permission",
                ClaimValue = "manage-users"
            };
            _context.AspNetRoleClaims.Add(claim);
            _context.SaveChanges();

            // Act
            claim.ClaimType = "admin-function";
            _service.Update(claim);

            // Assert
            var updated = _context.AspNetRoleClaims.Find(1);
            updated!.ClaimType.Should().Be("admin-function");
        }

        [Test]
        public void Update_DoesNotAffectOtherClaims()
        {
            // Arrange
            _context.AspNetRoleClaims.AddRange(
                new AspNetRoleClaim { Id = 1, RoleId = "admin", ClaimType = "permission", ClaimValue = "old-value" },
                new AspNetRoleClaim { Id = 2, RoleId = "admin", ClaimType = "permission", ClaimValue = "stable-value" }
            );
            _context.SaveChanges();

            var claim1 = _context.AspNetRoleClaims.Find(1);
            claim1!.ClaimValue = "new-value";

            // Act
            _service.Update(claim1);

            // Assert
            _context.AspNetRoleClaims.Find(2)!.ClaimValue.Should().Be("stable-value");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                Id = 1,
                RoleId = "admin",
                ClaimType = "permission",
                ClaimValue = "manage-users"
            };
            _context.AspNetRoleClaims.Add(claim);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.AspNetRoleClaims.Find(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(999);
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesCorrectClaimWhenMultipleExist()
        {
            // Arrange
            _context.AspNetRoleClaims.AddRange(
                new AspNetRoleClaim { Id = 1, RoleId = "admin", ClaimType = "permission", ClaimValue = "manage-users" },
                new AspNetRoleClaim { Id = 2, RoleId = "admin", ClaimType = "permission", ClaimValue = "manage-roles" },
                new AspNetRoleClaim { Id = 3, RoleId = "editor", ClaimType = "permission", ClaimValue = "edit-content" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.AspNetRoleClaims.Should().HaveCount(2);
            _context.AspNetRoleClaims.Find(2).Should().BeNull();
            _context.AspNetRoleClaims.Find(1).Should().NotBeNull();
            _context.AspNetRoleClaims.Find(3).Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void RoleClaims_FullLifecycle()
        {
            // Arrange
            var claim = new AspNetRoleClaim
            {
                RoleId = "new-role",
                ClaimType = "permission",
                ClaimValue = "original-permission"
            };

            // Act - Add
            _service.Add(claim);
            var added = _service.GetAll().FirstOrDefault(c => c.RoleId == "new-role");
            added.Should().NotBeNull();
            var claimId = added!.Id;

            // Act - Update
            var toUpdate = _service.Get(claimId);
            toUpdate!.ClaimValue = "updated-permission";
            _service.Update(toUpdate);
            var updated = _service.Get(claimId);
            updated!.ClaimValue.Should().Be("updated-permission");

            // Act - Delete
            _service.Delete(claimId);
            var deleted = _service.Get(claimId);
            deleted.Should().BeNull();
        }

        [Test]
        public void AdminRole_WithMultiplePermissions()
        {
            // Arrange
            var adminRole = "admin";
            var permissions = new[] { "manage-users", "manage-roles", "manage-content", "view-logs" };

            // Act - Assign permissions to admin role
            int idx = 1;
            foreach (var permission in permissions)
            {
                _service.Add(new AspNetRoleClaim
                {
                    RoleId = adminRole,
                    ClaimType = "permission",
                    ClaimValue = permission
                });
            }

            // Assert
            var allClaims = _service.GetAll();
            var adminClaims = allClaims.Where(c => c.RoleId == adminRole);
            adminClaims.Should().HaveCount(4);
            adminClaims.Select(c => c.ClaimValue).Should().Contain(permissions);
        }

        [Test]
        public void RolesWithDifferentPermissions()
        {
            // Arrange & Act
            _service.Add(new AspNetRoleClaim { RoleId = "admin", ClaimType = "permission", ClaimValue = "manage-users" });
            _service.Add(new AspNetRoleClaim { RoleId = "admin", ClaimType = "permission", ClaimValue = "manage-content" });
            _service.Add(new AspNetRoleClaim { RoleId = "editor", ClaimType = "permission", ClaimValue = "edit-content" });
            _service.Add(new AspNetRoleClaim { RoleId = "viewer", ClaimType = "permission", ClaimValue = "view-content" });

            // Assert
            var allClaims = _service.GetAll();
            allClaims.Where(c => c.RoleId == "admin").Should().HaveCount(2);
            allClaims.Where(c => c.RoleId == "editor").Should().HaveCount(1);
            allClaims.Where(c => c.RoleId == "viewer").Should().HaveCount(1);
        }

        #endregion
    }
}
