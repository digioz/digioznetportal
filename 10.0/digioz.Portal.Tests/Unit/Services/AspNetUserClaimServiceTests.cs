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
    /// Unit tests for AspNetUserClaimService - User claims management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Identity")]
    public class AspNetUserClaimServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AspNetUserClaimService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AspNetUserClaimService(_context);
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
            var claim = new AspNetUserClaim
            {
                Id = 1,
                UserId = "user-123",
                ClaimType = "email",
                ClaimValue = "user@example.com"
            };
            _context.AspNetUserClaims.Add(claim);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-123");
            result.ClaimType.Should().Be("email");
            result.ClaimValue.Should().Be("user@example.com");
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
            _context.AspNetUserClaims.AddRange(
                new AspNetUserClaim { Id = 1, UserId = "user-1", ClaimType = "email", ClaimValue = "user1@example.com" },
                new AspNetUserClaim { Id = 2, UserId = "user-1", ClaimType = "phone", ClaimValue = "+1234567890" },
                new AspNetUserClaim { Id = 3, UserId = "user-2", ClaimType = "email", ClaimValue = "user2@example.com" }
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
            var claim = new AspNetUserClaim
            {
                Id = 1,
                UserId = "user-1",
                ClaimType = "role",
                ClaimValue = "admin"
            };
            _context.AspNetUserClaims.Add(claim);
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(1);
            results[0].ClaimType.Should().Be("role");
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidClaim_AddsToDatabase()
        {
            // Arrange
            var claim = new AspNetUserClaim
            {
                UserId = "user-new",
                ClaimType = "email",
                ClaimValue = "newemail@example.com"
            };

            // Act
            _service.Add(claim);

            // Assert
            var saved = _context.AspNetUserClaims.FirstOrDefault(c => c.UserId == "user-new" && c.ClaimType == "email");
            saved.Should().NotBeNull();
            saved!.ClaimValue.Should().Be("newemail@example.com");
        }

        [Test]
        public void Add_MultipleClaimsForSameUser_AllAreSaved()
        {
            // Arrange
            var userId = "user-1";
            var claims = new[]
            {
                new AspNetUserClaim { UserId = userId, ClaimType = "email", ClaimValue = "user@example.com" },
                new AspNetUserClaim { UserId = userId, ClaimType = "phone", ClaimValue = "+1234567890" },
                new AspNetUserClaim { UserId = userId, ClaimType = "department", ClaimValue = "Engineering" }
            };

            // Act
            foreach (var claim in claims)
            {
                _service.Add(claim);
            }

            // Assert
            var allClaims = _service.GetAll();
            allClaims.Where(c => c.UserId == userId).Should().HaveCount(3);
        }

        [Test]
        public void Add_WithDifferentClaimTypes_AllAreStored()
        {
            // Arrange & Act
            _service.Add(new AspNetUserClaim { UserId = "user-1", ClaimType = "email", ClaimValue = "user@example.com" });
            _service.Add(new AspNetUserClaim { UserId = "user-1", ClaimType = "phone", ClaimValue = "123456" });
            _service.Add(new AspNetUserClaim { UserId = "user-1", ClaimType = "mobile", ClaimValue = "987654" });

            // Assert
            var allClaims = _service.GetAll();
            allClaims.Should().HaveCount(3);
            allClaims.Select(c => c.ClaimType).Should().Contain(new[] { "email", "phone", "mobile" });
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingClaim_UpdatesInDatabase()
        {
            // Arrange
            var claim = new AspNetUserClaim
            {
                Id = 1,
                UserId = "user-1",
                ClaimType = "email",
                ClaimValue = "old@example.com"
            };
            _context.AspNetUserClaims.Add(claim);
            _context.SaveChanges();

            // Act
            claim.ClaimValue = "new@example.com";
            _service.Update(claim);

            // Assert
            var updated = _context.AspNetUserClaims.Find(1);
            updated!.ClaimValue.Should().Be("new@example.com");
        }

        [Test]
        public void Update_WithClaimTypeChange_Updates()
        {
            // Arrange
            var claim = new AspNetUserClaim
            {
                Id = 1,
                UserId = "user-1",
                ClaimType = "email",
                ClaimValue = "user@example.com"
            };
            _context.AspNetUserClaims.Add(claim);
            _context.SaveChanges();

            // Act
            claim.ClaimType = "phone";
            _service.Update(claim);

            // Assert
            var updated = _context.AspNetUserClaims.Find(1);
            updated!.ClaimType.Should().Be("phone");
        }

        [Test]
        public void Update_DoesNotAffectOtherClaims()
        {
            // Arrange
            _context.AspNetUserClaims.AddRange(
                new AspNetUserClaim { Id = 1, UserId = "user-1", ClaimType = "email", ClaimValue = "old@example.com" },
                new AspNetUserClaim { Id = 2, UserId = "user-1", ClaimType = "phone", ClaimValue = "1234567890" }
            );
            _context.SaveChanges();

            var claim1 = _context.AspNetUserClaims.Find(1);
            claim1!.ClaimValue = "new@example.com";

            // Act
            _service.Update(claim1);

            // Assert
            _context.AspNetUserClaims.Find(2)!.ClaimValue.Should().Be("1234567890");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var claim = new AspNetUserClaim
            {
                Id = 1,
                UserId = "user-1",
                ClaimType = "email",
                ClaimValue = "user@example.com"
            };
            _context.AspNetUserClaims.Add(claim);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.AspNetUserClaims.Find(1);
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
            _context.AspNetUserClaims.AddRange(
                new AspNetUserClaim { Id = 1, UserId = "user-1", ClaimType = "email", ClaimValue = "user@example.com" },
                new AspNetUserClaim { Id = 2, UserId = "user-1", ClaimType = "phone", ClaimValue = "1234567890" },
                new AspNetUserClaim { Id = 3, UserId = "user-2", ClaimType = "email", ClaimValue = "other@example.com" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.AspNetUserClaims.Should().HaveCount(2);
            _context.AspNetUserClaims.Find(2).Should().BeNull();
            _context.AspNetUserClaims.Find(1).Should().NotBeNull();
            _context.AspNetUserClaims.Find(3).Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void UserClaims_FullLifecycle()
        {
            // Arrange
            var claim = new AspNetUserClaim
            {
                UserId = "user-lifecycle",
                ClaimType = "custom-claim",
                ClaimValue = "original-value"
            };

            // Act - Add
            _service.Add(claim);
            var added = _service.GetAll().FirstOrDefault(c => c.UserId == "user-lifecycle");
            added.Should().NotBeNull();
            var claimId = added!.Id;

            // Act - Update
            var toUpdate = _service.Get(claimId);
            toUpdate!.ClaimValue = "updated-value";
            _service.Update(toUpdate);
            var updated = _service.Get(claimId);
            updated!.ClaimValue.Should().Be("updated-value");

            // Act - Delete
            _service.Delete(claimId);
            var deleted = _service.Get(claimId);
            deleted.Should().BeNull();
        }

        [Test]
        public void UserWithMultipleClaims_CanBeManaged()
        {
            // Arrange
            var userId = "user-complete";
            var claimTypes = new[] { "email", "phone", "department", "role" };

            // Act - Add multiple claims
            int idx = 1;
            foreach (var claimType in claimTypes)
            {
                _service.Add(new AspNetUserClaim
                {
                    UserId = userId,
                    ClaimType = claimType,
                    ClaimValue = $"value-{idx++}"
                });
            }

            // Assert
            var allClaims = _service.GetAll();
            var userClaims = allClaims.Where(c => c.UserId == userId);
            userClaims.Should().HaveCount(4);
            userClaims.Select(c => c.ClaimType).Should().Contain(claimTypes);
        }

        [Test]
        public void CommonClaims_CanBeUsedAcrossUsers()
        {
            // Arrange & Act - Both users have "role" claim
            _service.Add(new AspNetUserClaim { UserId = "user-1", ClaimType = "role", ClaimValue = "admin" });
            _service.Add(new AspNetUserClaim { UserId = "user-2", ClaimType = "role", ClaimValue = "user" });

            // Assert
            var allClaims = _service.GetAll();
            allClaims.Where(c => c.ClaimType == "role").Should().HaveCount(2);
        }

        #endregion
    }
}
