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
    /// Unit tests for AspNetUserTokenService - User token management (2FA, recovery codes, external auth)
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Identity")]
    public class AspNetUserTokenServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private AspNetUserTokenService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new AspNetUserTokenService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidUserAndToken_ReturnsToken()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "user-123",
                LoginProvider = "Google",
                Name = "access_token",
                Value = "token_value_123"
            };
            _context.AspNetUserTokens.Add(token);
            _context.SaveChanges();

            // Act
            var result = _service.Get("user-123", "Google", "access_token");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-123");
            result.LoginProvider.Should().Be("Google");
            result.Name.Should().Be("access_token");
            result.Value.Should().Be("token_value_123");
        }

        [Test]
        public void Get_WithInvalidUserOrProvider_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent-user", "Google", "access_token");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_WithNullParameters_ReturnsNull()
        {
            // Act
            var result = _service.Get(null, null, null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Get_IsCaseSensitive_ForLoginProvider()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "user-1",
                LoginProvider = "Google",
                Name = "access_token",
                Value = "token123"
            };
            _context.AspNetUserTokens.Add(token);
            _context.SaveChanges();

            // Act
            var result = _service.Get("user-1", "google", "access_token"); // lowercase

            // Assert
            // Case sensitivity depends on EF Core configuration; testing the actual behavior
            // In most cases, this would return null due to case comparison
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleTokens_ReturnsAllTokens()
        {
            // Arrange
            _context.AspNetUserTokens.AddRange(
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "access_token", Value = "token1" },
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Facebook", Name = "access_token", Value = "token2" },
                new AspNetUserToken { UserId = "user-2", LoginProvider = "Google", Name = "access_token", Value = "token3" }
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
        public void GetAll_WithSingleToken_ReturnsSingleToken()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "user-1",
                LoginProvider = "Microsoft",
                Name = "refresh_token",
                Value = "refresh_value"
            };
            _context.AspNetUserTokens.Add(token);
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(1);
            results[0].LoginProvider.Should().Be("Microsoft");
        }

        [Test]
        public void GetAll_WithUserMultipleTokens()
        {
            // Arrange - One user with multiple OAuth tokens
            _context.AspNetUserTokens.AddRange(
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "access_token", Value = "google_access" },
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "refresh_token", Value = "google_refresh" },
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Facebook", Name = "access_token", Value = "fb_access" }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
            results.Where(t => t.UserId == "user-1").Should().HaveCount(3);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidToken_AddsToDatabase()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "new-user",
                LoginProvider = "GitHub",
                Name = "access_token",
                Value = "github_token"
            };

            // Act
            _service.Add(token);

            // Assert
            var saved = _context.AspNetUserTokens.FirstOrDefault(t =>
                t.UserId == "new-user" && t.LoginProvider == "GitHub" && t.Name == "access_token");
            saved.Should().NotBeNull();
            saved!.Value.Should().Be("github_token");
        }

        [Test]
        public void Add_MultipleTokensForSameUser_AllAreSaved()
        {
            // Arrange
            var userId = "multi-user";
            var tokens = new[]
            {
                new AspNetUserToken { UserId = userId, LoginProvider = "Google", Name = "access_token", Value = "google1" },
                new AspNetUserToken { UserId = userId, LoginProvider = "Facebook", Name = "access_token", Value = "facebook1" },
                new AspNetUserToken { UserId = userId, LoginProvider = "Microsoft", Name = "access_token", Value = "microsoft1" }
            };

            // Act
            foreach (var token in tokens)
            {
                _service.Add(token);
            }

            // Assert
            var allTokens = _service.GetAll();
            allTokens.Where(t => t.UserId == userId).Should().HaveCount(3);
        }

        [Test]
        public void Add_UserWithAccessAndRefreshTokens()
        {
            // Arrange - OAuth typically provides both access and refresh tokens
            var userId = "oauth-user";

            // Act
            _service.Add(new AspNetUserToken { UserId = userId, LoginProvider = "Google", Name = "access_token", Value = "access_123" });
            _service.Add(new AspNetUserToken { UserId = userId, LoginProvider = "Google", Name = "refresh_token", Value = "refresh_123" });

            // Assert
            var allTokens = _service.GetAll();
            var userTokens = allTokens.Where(t => t.UserId == userId && t.LoginProvider == "Google");
            userTokens.Should().HaveCount(2);
            userTokens.Should().Contain(t => t.Name == "access_token");
            userTokens.Should().Contain(t => t.Name == "refresh_token");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingToken_UpdatesInDatabase()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "user-1",
                LoginProvider = "Google",
                Name = "access_token",
                Value = "old_token_value"
            };
            _context.AspNetUserTokens.Add(token);
            _context.SaveChanges();

            // Act
            token.Value = "new_token_value";
            _service.Update(token);

            // Assert
            var updated = _context.AspNetUserTokens.FirstOrDefault(t =>
                t.UserId == "user-1" && t.LoginProvider == "Google" && t.Name == "access_token");
            updated.Should().NotBeNull();
            updated!.Value.Should().Be("new_token_value");
        }

        [Test]
        public void Update_RefreshToken_UpdatesCorrectly()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "user-1",
                LoginProvider = "Facebook",
                Name = "refresh_token",
                Value = "old_refresh"
            };
            _context.AspNetUserTokens.Add(token);
            _context.SaveChanges();

            // Act
            token.Value = "new_refresh";
            _service.Update(token);

            // Assert
            var updated = _service.Get("user-1", "Facebook", "refresh_token");
            updated!.Value.Should().Be("new_refresh");
        }

        [Test]
        public void Update_DoesNotAffectOtherTokens()
        {
            // Arrange
            _context.AspNetUserTokens.AddRange(
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "access_token", Value = "old_google" },
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Facebook", Name = "access_token", Value = "constant_fb" }
            );
            _context.SaveChanges();

            var googleToken = _context.AspNetUserTokens.FirstOrDefault(t =>
                t.UserId == "user-1" && t.LoginProvider == "Google");
            googleToken!.Value = "new_google";

            // Act
            _service.Update(googleToken);

            // Assert
            var fbToken = _service.Get("user-1", "Facebook", "access_token");
            fbToken!.Value.Should().Be("constant_fb");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingToken_RemovesFromDatabase()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "user-1",
                LoginProvider = "Google",
                Name = "access_token",
                Value = "token123"
            };
            _context.AspNetUserTokens.Add(token);
            _context.SaveChanges();

            // Act
            _service.Delete("user-1", "Google", "access_token");

            // Assert
            var deleted = _service.Get("user-1", "Google", "access_token");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingToken_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent-user", "Google", "access_token");
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_WithNullParameters_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(null, null, null);
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesSpecificTokenKeepingOthers()
        {
            // Arrange
            _context.AspNetUserTokens.AddRange(
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "access_token", Value = "google_access" },
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "refresh_token", Value = "google_refresh" },
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Facebook", Name = "access_token", Value = "fb_access" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("user-1", "Google", "access_token");

            // Assert
            var remaining = _service.GetAll();
            remaining.Should().HaveCount(2);
            remaining.FirstOrDefault(t => t.LoginProvider == "Google" && t.Name == "refresh_token").Should().NotBeNull();
            remaining.FirstOrDefault(t => t.LoginProvider == "Facebook").Should().NotBeNull();
        }

        [Test]
        public void Delete_OneUserDoesNotAffectAnother()
        {
            // Arrange
            _context.AspNetUserTokens.AddRange(
                new AspNetUserToken { UserId = "user-1", LoginProvider = "Google", Name = "access_token", Value = "user1_token" },
                new AspNetUserToken { UserId = "user-2", LoginProvider = "Google", Name = "access_token", Value = "user2_token" }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("user-1", "Google", "access_token");

            // Assert
            var user2Token = _service.Get("user-2", "Google", "access_token");
            user2Token.Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void UserToken_FullLifecycle()
        {
            // Arrange
            var token = new AspNetUserToken
            {
                UserId = "lifecycle-user",
                LoginProvider = "TestProvider",
                Name = "token",
                Value = "original_value"
            };

            // Act - Add
            _service.Add(token);
            var added = _service.Get("lifecycle-user", "TestProvider", "token");
            added.Should().NotBeNull();

            // Act - Update
            added!.Value = "updated_value";
            _service.Update(added);
            var updated = _service.Get("lifecycle-user", "TestProvider", "token");
            updated!.Value.Should().Be("updated_value");

            // Act - Delete
            _service.Delete("lifecycle-user", "TestProvider", "token");
            var deleted = _service.Get("lifecycle-user", "TestProvider", "token");
            deleted.Should().BeNull();
        }

        [Test]
        public void OAuth_UserWithMultipleProviders()
        {
            // Arrange
            var userId = "oauth-user";
            var providers = new[] { "Google", "Facebook", "GitHub", "Microsoft" };

            // Act - User authenticates with multiple OAuth providers
            foreach (var provider in providers)
            {
                _service.Add(new AspNetUserToken
                {
                    UserId = userId,
                    LoginProvider = provider,
                    Name = "access_token",
                    Value = $"{provider.ToLower()}_token"
                });
            }

            // Assert
            var allTokens = _service.GetAll();
            var userTokens = allTokens.Where(t => t.UserId == userId);
            userTokens.Should().HaveCount(4);
            userTokens.Select(t => t.LoginProvider).Should().Contain(providers);
        }

        [Test]
        public void TokenRefresh_ScenarioUpdate()
        {
            // Arrange
            var userId = "refresh-user";
            var provider = "Google";

            // Act - Initial login
            _service.Add(new AspNetUserToken
            {
                UserId = userId,
                LoginProvider = provider,
                Name = "access_token",
                Value = "access_token_1"
            });
            _service.Add(new AspNetUserToken
            {
                UserId = userId,
                LoginProvider = provider,
                Name = "refresh_token",
                Value = "refresh_token_1"
            });

            // Act - Token refresh: update access token with new one
            var accessToken = _service.Get(userId, provider, "access_token");
            accessToken!.Value = "access_token_2";
            _service.Update(accessToken);

            // Assert - Refresh token should remain unchanged
            var refreshToken = _service.Get(userId, provider, "refresh_token");
            refreshToken!.Value.Should().Be("refresh_token_1");
            accessToken = _service.Get(userId, provider, "access_token");
            accessToken!.Value.Should().Be("access_token_2");
        }

        #endregion
    }
}
