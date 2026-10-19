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
    /// Unit tests for ThemeService - Site theme/stylesheet management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Theming")]
    public class ThemeServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ThemeService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ThemeService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsTheme()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Blue Theme",
                Body = "body { background-color: #0066cc; }",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };
            _context.Themes.Add(theme);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Blue Theme");
            result.Body.Should().Contain("0066cc");
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

        #region GetDefault Tests

        [Test]
        public void GetDefault_WithDefaultThemeSet_ReturnsDefault()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Blue", Body = "blue css", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Green", Body = "green css", CreateDate = DateTime.UtcNow, IsDefault = true },
                new Theme { Id = 3, Name = "Red", Body = "red css", CreateDate = DateTime.UtcNow, IsDefault = false }
            );
            _context.SaveChanges();

            // Act
            var result = _service.GetDefault();

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Green");
            result.IsDefault.Should().BeTrue();
        }

        [Test]
        public void GetDefault_WithNoDefaultTheme_ReturnsNull()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Theme 1", Body = "css1", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Theme 2", Body = "css2", CreateDate = DateTime.UtcNow, IsDefault = false }
            );
            _context.SaveChanges();

            // Act
            var result = _service.GetDefault();

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleThemes_ReturnsAll()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Light", Body = "light css", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Dark", Body = "dark css", CreateDate = DateTime.UtcNow, IsDefault = true },
                new Theme { Id = 3, Name = "Blue", Body = "blue css", CreateDate = DateTime.UtcNow, IsDefault = false }
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

        #region Add Tests

        [Test]
        public void Add_WithValidTheme_AddsToDatabase()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Custom Theme",
                Body = "body { font-family: Arial; color: #333; }",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };

            // Act
            _service.Add(theme);

            // Assert
            var saved = _context.Themes.Find(1);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Custom Theme");
        }

        [Test]
        public void Add_WithComplexCss_PreservesCss()
        {
            // Arrange
            var css = @"
                body {
                    background-color: #f5f5f5;
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    color: #333;
                }
                .container {
                    max-width: 1200px;
                    margin: 0 auto;
                    padding: 20px;
                }
                .btn {
                    background-color: #007bff;
                    color: white;
                    padding: 10px 20px;
                    border-radius: 4px;
                    cursor: pointer;
                }";

            var theme = new Theme
            {
                Id = 1,
                Name = "Modern Theme",
                Body = css,
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };

            // Act
            _service.Add(theme);

            // Assert
            var saved = _context.Themes.Find(1);
            saved!.Body.Should().Contain("1200px");
            saved.Body.Should().Contain("007bff");
        }

        [Test]
        public void Add_AsDefault_Saves()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Default Theme",
                Body = "default css",
                CreateDate = DateTime.UtcNow,
                IsDefault = true
            };

            // Act
            _service.Add(theme);

            // Assert
            var saved = _context.Themes.Find(1);
            saved!.IsDefault.Should().BeTrue();
        }

        [Test]
        public void Add_WithCreateDate_PreservesDate()
        {
            // Arrange
            var createDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var theme = new Theme
            {
                Id = 1,
                Name = "Dated Theme",
                Body = "css",
                CreateDate = createDate,
                IsDefault = false
            };

            // Act
            _service.Add(theme);

            // Assert
            var saved = _context.Themes.Find(1);
            saved!.CreateDate.Should().Be(createDate);
        }

        [Test]
        public void Add_MultipleThemes_AllAreSaved()
        {
            // Arrange
            var themes = new[]
            {
                new Theme { Id = 1, Name = "Theme 1", Body = "css1", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Theme 2", Body = "css2", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 3, Name = "Theme 3", Body = "css3", CreateDate = DateTime.UtcNow, IsDefault = true }
            };

            // Act
            foreach (var theme in themes)
            {
                _service.Add(theme);
            }

            // Assert
            _context.Themes.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingTheme_UpdatesInDatabase()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Old Name",
                Body = "old css",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };
            _context.Themes.Add(theme);
            _context.SaveChanges();

            // Act
            theme.Name = "New Name";
            theme.Body = "new css";
            theme.IsDefault = true;
            _service.Update(theme);

            // Assert
            var updated = _context.Themes.Find(1);
            updated!.Name.Should().Be("New Name");
            updated.Body.Should().Be("new css");
            updated.IsDefault.Should().BeTrue();
        }

        [Test]
        public void Update_ChangeCss_Updates()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Updateable Theme",
                Body = "body { color: red; }",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };
            _context.Themes.Add(theme);
            _context.SaveChanges();

            // Act
            theme.Body = "body { color: blue; }";
            _service.Update(theme);

            // Assert
            var updated = _context.Themes.Find(1);
            updated!.Body.Should().Contain("blue");
        }

        [Test]
        public void Update_ChangeDefault_Updates()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Theme",
                Body = "css",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };
            _context.Themes.Add(theme);
            _context.SaveChanges();

            // Act
            theme.IsDefault = true;
            _service.Update(theme);

            // Assert
            var updated = _context.Themes.Find(1);
            updated!.IsDefault.Should().BeTrue();
        }

        [Test]
        public void Update_DoesNotAffectOtherThemes()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Theme 1", Body = "css1", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Theme 2", Body = "css2", CreateDate = DateTime.UtcNow, IsDefault = false }
            );
            _context.SaveChanges();

            var theme1 = _context.Themes.Find(1);
            theme1!.Name = "Updated Theme 1";

            // Act
            _service.Update(theme1);

            // Assert
            _context.Themes.Find(2)!.Name.Should().Be("Theme 2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Theme to Delete",
                Body = "temp css",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };
            _context.Themes.Add(theme);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Themes.Find(1);
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
        public void Delete_RemovesCorrectThemeWhenMultipleExist()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Theme 1", Body = "css1", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Theme 2", Body = "css2", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 3, Name = "Theme 3", Body = "css3", CreateDate = DateTime.UtcNow, IsDefault = false }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.Themes.Should().HaveCount(2);
            _context.Themes.Find(2).Should().BeNull();
            _context.Themes.Find(1).Should().NotBeNull();
            _context.Themes.Find(3).Should().NotBeNull();
        }

        #endregion

        #region SetAsDefault Tests

        [Test]
        [Ignore("SetAsDefault uses ExecuteUpdate which is not supported by EF Core InMemory provider")]
        public void SetAsDefault_WithValidId_SetsAsDefault()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Light", Body = "light", CreateDate = DateTime.UtcNow, IsDefault = true },
                new Theme { Id = 2, Name = "Dark", Body = "dark", CreateDate = DateTime.UtcNow, IsDefault = false }
            );
            _context.SaveChanges();

            // Act
            _service.SetAsDefault(2);

            // Assert
            var dark = _context.Themes.Find(2);
            var light = _context.Themes.Find(1);
            dark!.IsDefault.Should().BeTrue();
            light!.IsDefault.Should().BeFalse();
        }

        [Test]
        [Ignore("SetAsDefault uses ExecuteUpdate which is not supported by EF Core InMemory provider")]
        public void SetAsDefault_ClearsOtherDefault()
        {
            // Arrange
            _context.Themes.AddRange(
                new Theme { Id = 1, Name = "Theme 1", Body = "css1", CreateDate = DateTime.UtcNow, IsDefault = true },
                new Theme { Id = 2, Name = "Theme 2", Body = "css2", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 3, Name = "Theme 3", Body = "css3", CreateDate = DateTime.UtcNow, IsDefault = false }
            );
            _context.SaveChanges();

            // Act
            _service.SetAsDefault(3);

            // Assert
            var allThemes = _service.GetAll();
            var defaultThemes = allThemes.Where(t => t.IsDefault).ToList();
            defaultThemes.Should().HaveCount(1);
            defaultThemes.First().Id.Should().Be(3);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Theme_FullLifecycle()
        {
            // Arrange
            var theme = new Theme
            {
                Id = 1,
                Name = "Lifecycle Theme",
                Body = "initial css",
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };

            // Act - Add
            _service.Add(theme);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Theme";
            added.Body = "updated css";
            added.IsDefault = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Name.Should().Be("Updated Lifecycle Theme");
            updated.IsDefault.Should().BeTrue();

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageThemeSelection_WithMultipleThemes()
        {
            // Arrange
            var themes = new[]
            {
                new Theme { Id = 1, Name = "Light", Body = "light colors", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 2, Name = "Dark", Body = "dark colors", CreateDate = DateTime.UtcNow, IsDefault = false },
                new Theme { Id = 3, Name = "High Contrast", Body = "high contrast colors", CreateDate = DateTime.UtcNow, IsDefault = true }
            };

            // Act
            foreach (var theme in themes)
            {
                _service.Add(theme);
            }

            var defaultTheme = _service.GetDefault();

            // Assert
            defaultTheme!.Name.Should().Be("High Contrast");

            // Note: SetAsDefault uses ExecuteUpdate which is not supported by InMemory,
            // so we cannot test the ability to change the default theme in this context
        }

        [Test]
        public void AllThemesVisible_InGetAll()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                _service.Add(new Theme
                {
                    Id = i,
                    Name = $"Theme {i}",
                    Body = $"css {i}",
                    CreateDate = DateTime.UtcNow,
                    IsDefault = i == 3
                });
            }

            // Act
            var allThemes = _service.GetAll();
            var defaultTheme = _service.GetDefault();

            // Assert
            allThemes.Should().HaveCount(5);
            defaultTheme!.Id.Should().Be(3);
        }

        [Test]
        public void ThemeVersioning_WithMultipleCss()
        {
            // Arrange
            var cssV1 = "body { color: #000; }";
            var cssV2 = "body { color: #333; } .new { display: none; }";
            var cssV3 = "body { color: #666; } .old { display: block; } .new { display: flex; }";

            var theme = new Theme
            {
                Id = 1,
                Name = "Evolving Theme",
                Body = cssV1,
                CreateDate = DateTime.UtcNow,
                IsDefault = false
            };

            // Act - Add first version
            _service.Add(theme);

            // Act - Update to version 2
            var updated = _service.Get(1);
            updated!.Body = cssV2;
            _service.Update(updated);

            // Act - Update to version 3
            updated = _service.Get(1);
            updated.Body = cssV3;
            _service.Update(updated);

            var final = _service.Get(1);

            // Assert - verify final version was updated correctly
            final!.Body.Should().Be(cssV3);
            final.Body.Should().Contain("#666");
            final.Body.Should().Contain(".old");
            final.Body.Should().Contain("flex");
        }

        #endregion
    }
}
