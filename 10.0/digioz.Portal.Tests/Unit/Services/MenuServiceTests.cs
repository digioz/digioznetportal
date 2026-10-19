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
    /// Unit tests for MenuService - Navigation menu management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Navigation")]
    public class MenuServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private MenuService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new MenuService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsMenu()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Home",
                Location = "Header",
                Controller = "Home",
                Action = "Index",
                Url = "/",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 1
            };
            _context.Menus.Add(menu);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Home");
            result.Location.Should().Be("Header");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetNoTracking_WithValidId_ReturnsMenu()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "About",
                Location = "Header",
                Controller = "About",
                Action = "Index",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 2
            };
            _context.Menus.Add(menu);
            _context.SaveChanges();

            // Act
            var result = _service.GetNoTracking(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("About");
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleMenus_ReturnsAll()
        {
            // Arrange
            _context.Menus.AddRange(
                new Menu { Id = 1, Name = "Home", Location = "Header", Controller = "Home", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 },
                new Menu { Id = 2, Name = "About", Location = "Header", Controller = "About", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 },
                new Menu { Id = 3, Name = "Contact", Location = "Header", Controller = "Contact", Action = "Index", Visible = false, Timestamp = DateTime.UtcNow, SortOrder = 3 }
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
        public void Add_WithValidMenu_AddsToDatabase()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Services",
                Location = "Header",
                Controller = "Services",
                Action = "Index",
                Url = "/services",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 4
            };

            // Act
            _service.Add(menu);

            // Assert
            var saved = _context.Menus.Find(1);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Services");
        }

        [Test]
        public void Add_WithUrlAndControllerAction_Saves()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "External Link",
                Location = "Footer",
                Controller = null,
                Action = null,
                Url = "https://example.com",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 100
            };

            // Act
            _service.Add(menu);

            // Assert
            var saved = _context.Menus.Find(1);
            saved!.Url.Should().Be("https://example.com");
        }

        [Test]
        public void Add_WithVisibility_Saves()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Hidden Menu",
                Location = "Header",
                Controller = "Admin",
                Action = "Dashboard",
                Visible = false,
                Timestamp = DateTime.UtcNow,
                SortOrder = 200
            };

            // Act
            _service.Add(menu);

            // Assert
            var saved = _context.Menus.Find(1);
            saved!.Visible.Should().BeFalse();
        }

        [Test]
        public void Add_MultipleMenus_AllAreSaved()
        {
            // Arrange
            var menus = new[]
            {
                new Menu { Id = 1, Name = "Home", Location = "Header", Controller = "Home", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 },
                new Menu { Id = 2, Name = "About", Location = "Header", Controller = "About", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 },
                new Menu { Id = 3, Name = "Contact", Location = "Footer", Controller = "Contact", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 3 }
            };

            // Act
            foreach (var menu in menus)
            {
                _service.Add(menu);
            }

            // Assert
            _context.Menus.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingMenu_UpdatesInDatabase()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Original Name",
                Location = "Header",
                Controller = "Original",
                Action = "Index",
                Visible = false,
                Timestamp = DateTime.UtcNow,
                SortOrder = 1
            };
            _context.Menus.Add(menu);
            _context.SaveChanges();

            // Act
            menu.Name = "Updated Name";
            menu.Controller = "Updated";
            menu.Visible = true;
            menu.SortOrder = 5;
            _service.Update(menu);

            // Assert
            var updated = _context.Menus.Find(1);
            updated!.Name.Should().Be("Updated Name");
            updated.Controller.Should().Be("Updated");
            updated.Visible.Should().BeTrue();
            updated.SortOrder.Should().Be(5);
        }

        [Test]
        public void Update_ChangeVisibility_Updates()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Menu",
                Location = "Header",
                Controller = "Home",
                Action = "Index",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 1
            };
            _context.Menus.Add(menu);
            _context.SaveChanges();

            // Act
            menu.Visible = false;
            _service.Update(menu);

            // Assert
            var updated = _context.Menus.Find(1);
            updated!.Visible.Should().BeFalse();
        }

        [Test]
        public void Update_ChangeSortOrder_Updates()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Menu",
                Location = "Header",
                Controller = "Home",
                Action = "Index",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 1
            };
            _context.Menus.Add(menu);
            _context.SaveChanges();

            // Act
            menu.SortOrder = 99;
            _service.Update(menu);

            // Assert
            var updated = _context.Menus.Find(1);
            updated!.SortOrder.Should().Be(99);
        }

        [Test]
        public void Update_DoesNotAffectOtherMenus()
        {
            // Arrange
            _context.Menus.AddRange(
                new Menu { Id = 1, Name = "Menu 1", Location = "Header", Controller = "Home", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 },
                new Menu { Id = 2, Name = "Menu 2", Location = "Header", Controller = "About", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 }
            );
            _context.SaveChanges();

            var menu1 = _context.Menus.Find(1);
            menu1!.Name = "Updated Menu 1";

            // Act
            _service.Update(menu1);

            // Assert
            _context.Menus.Find(2)!.Name.Should().Be("Menu 2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Menu to Delete",
                Location = "Header",
                Controller = "Temp",
                Action = "Index",
                Visible = true,
                Timestamp = DateTime.UtcNow,
                SortOrder = 1
            };
            _context.Menus.Add(menu);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Menus.Find(1);
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
        public void Delete_RemovesCorrectMenuWhenMultipleExist()
        {
            // Arrange
            _context.Menus.AddRange(
                new Menu { Id = 1, Name = "Menu 1", Location = "Header", Controller = "H1", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 },
                new Menu { Id = 2, Name = "Menu 2", Location = "Header", Controller = "H2", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 },
                new Menu { Id = 3, Name = "Menu 3", Location = "Header", Controller = "H3", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 3 }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.Menus.Should().HaveCount(2);
            _context.Menus.Find(2).Should().BeNull();
            _context.Menus.Find(1).Should().NotBeNull();
            _context.Menus.Find(3).Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Menu_FullLifecycle()
        {
            // Arrange
            var menu = new Menu
            {
                Id = 1,
                Name = "Lifecycle Menu",
                Location = "Header",
                Controller = "Lifecycle",
                Action = "Test",
                Visible = false,
                Timestamp = DateTime.UtcNow,
                SortOrder = 100
            };

            // Act - Add
            _service.Add(menu);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Menu";
            added.Visible = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Name.Should().Be("Updated Lifecycle Menu");
            updated.Visible.Should().BeTrue();

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageNavigationMenu_WithMultipleLocations()
        {
            // Arrange - Create menus for different locations
            var headerMenus = new[]
            {
                new Menu { Id = 1, Name = "Home", Location = "Header", Controller = "Home", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 },
                new Menu { Id = 2, Name = "About", Location = "Header", Controller = "About", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 }
            };
            var footerMenus = new[]
            {
                new Menu { Id = 3, Name = "Privacy", Location = "Footer", Controller = "Privacy", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 },
                new Menu { Id = 4, Name = "Terms", Location = "Footer", Controller = "Terms", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 }
            };

            // Act
            foreach (var menu in headerMenus.Concat(footerMenus))
            {
                _service.Add(menu);
            }

            var allMenus = _service.GetAll();
            var header = allMenus.Where(m => m.Location == "Header").ToList();
            var footer = allMenus.Where(m => m.Location == "Footer").ToList();

            // Assert
            allMenus.Should().HaveCount(4);
            header.Should().HaveCount(2);
            footer.Should().HaveCount(2);
        }

        [Test]
        public void MenuVisibility_CanBeControlled()
        {
            // Arrange
            _service.Add(new Menu { Id = 1, Name = "Visible Menu", Location = "Header", Controller = "Home", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 });
            _service.Add(new Menu { Id = 2, Name = "Hidden Menu", Location = "Header", Controller = "Admin", Action = "Panel", Visible = false, Timestamp = DateTime.UtcNow, SortOrder = 2 });

            // Act
            var allMenus = _service.GetAll();
            var visible = allMenus.Where(m => m.Visible).ToList();
            var hidden = allMenus.Where(m => !m.Visible).ToList();

            // Assert
            visible.Should().HaveCount(1);
            hidden.Should().HaveCount(1);
        }

        [Test]
        public void MenuSortOrder_ControlsDisplayOrder()
        {
            // Arrange
            _service.Add(new Menu { Id = 1, Name = "First", Location = "Header", Controller = "First", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 1 });
            _service.Add(new Menu { Id = 2, Name = "Second", Location = "Header", Controller = "Second", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 2 });
            _service.Add(new Menu { Id = 3, Name = "Third", Location = "Header", Controller = "Third", Action = "Index", Visible = true, Timestamp = DateTime.UtcNow, SortOrder = 3 });

            // Act
            var menus = _service.GetAll().OrderBy(m => m.SortOrder).ToList();

            // Assert
            menus[0].Name.Should().Be("First");
            menus[1].Name.Should().Be("Second");
            menus[2].Name.Should().Be("Third");
        }

        #endregion
    }
}
