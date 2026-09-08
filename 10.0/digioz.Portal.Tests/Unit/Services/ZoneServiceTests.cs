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
    /// Unit tests for ZoneService - Content zone management (sidebar, content areas, etc.)
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Content")]
    public class ZoneServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private ZoneService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new ZoneService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsZone()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Sidebar Left",
                Location = "Left",
                Body = "<h3>Sidebar Widget</h3>",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Zones.Add(zone);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Sidebar Left");
            result.Location.Should().Be("Left");
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
        public void GetAll_WithMultipleZones_ReturnsAll()
        {
            // Arrange
            _context.Zones.AddRange(
                new Zone { Id = 1, Name = "Sidebar Left", Location = "Left", Body = "Content 1", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 2, Name = "Sidebar Right", Location = "Right", Body = "Content 2", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 3, Name = "Footer", Location = "Bottom", Body = "Content 3", Visible = false, Timestamp = DateTime.UtcNow }
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
        public void Add_WithValidZone_AddsToDatabase()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Header Top",
                Location = "Top",
                Body = "<div class='header'>Website Banner</div>",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(zone);

            // Assert
            var saved = _context.Zones.Find(1);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Header Top");
            saved.Body.Should().Contain("header");
        }

        [Test]
        public void Add_WithHtmlContent_PreservesHtml()
        {
            // Arrange
            var htmlContent = "<div class='widget'><h3>Recent Posts</h3><ul><li>Post 1</li><li>Post 2</li></ul></div>";
            var zone = new Zone
            {
                Id = 1,
                Name = "Widget Zone",
                Location = "Main",
                Body = htmlContent,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(zone);

            // Assert
            var saved = _context.Zones.Find(1);
            saved!.Body.Should().Be(htmlContent);
        }

        [Test]
        public void Add_WithVisibility_Saves()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Hidden Zone",
                Location = "Secret",
                Body = "Hidden content",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(zone);

            // Assert
            var saved = _context.Zones.Find(1);
            saved!.Visible.Should().BeFalse();
        }

        [Test]
        public void Add_MultipleZones_AllAreSaved()
        {
            // Arrange
            var zones = new[]
            {
                new Zone { Id = 1, Name = "Zone 1", Location = "Loc1", Body = "Body 1", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 2, Name = "Zone 2", Location = "Loc2", Body = "Body 2", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 3, Name = "Zone 3", Location = "Loc3", Body = "Body 3", Visible = false, Timestamp = DateTime.UtcNow }
            };

            // Act
            foreach (var zone in zones)
            {
                _service.Add(zone);
            }

            // Assert
            _context.Zones.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingZone_UpdatesInDatabase()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Original Name",
                Location = "Left",
                Body = "Original content",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };
            _context.Zones.Add(zone);
            _context.SaveChanges();

            // Act
            zone.Name = "Updated Name";
            zone.Body = "Updated content";
            zone.Visible = true;
            _service.Update(zone);

            // Assert
            var updated = _context.Zones.Find(1);
            updated!.Name.Should().Be("Updated Name");
            updated.Body.Should().Be("Updated content");
            updated.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_ChangeContent_Updates()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Widget Zone",
                Location = "Sidebar",
                Body = "<p>Old widget code</p>",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Zones.Add(zone);
            _context.SaveChanges();

            // Act
            zone.Body = "<p>New widget code</p>";
            _service.Update(zone);

            // Assert
            var updated = _context.Zones.Find(1);
            updated!.Body.Should().Contain("New widget");
        }

        [Test]
        public void Update_ChangeVisibility_Updates()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Zone",
                Location = "Side",
                Body = "Content",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Zones.Add(zone);
            _context.SaveChanges();

            // Act
            zone.Visible = false;
            _service.Update(zone);

            // Assert
            var updated = _context.Zones.Find(1);
            updated!.Visible.Should().BeFalse();
        }

        [Test]
        public void Update_DoesNotAffectOtherZones()
        {
            // Arrange
            _context.Zones.AddRange(
                new Zone { Id = 1, Name = "Zone 1", Location = "L1", Body = "Body 1", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 2, Name = "Zone 2", Location = "L2", Body = "Body 2", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            var zone1 = _context.Zones.Find(1);
            zone1!.Name = "Updated Zone 1";

            // Act
            _service.Update(zone1);

            // Assert
            _context.Zones.Find(2)!.Name.Should().Be("Zone 2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Zone to Delete",
                Location = "Temp",
                Body = "Temporary content",
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.Zones.Add(zone);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Zones.Find(1);
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
        public void Delete_RemovesCorrectZoneWhenMultipleExist()
        {
            // Arrange
            _context.Zones.AddRange(
                new Zone { Id = 1, Name = "Zone 1", Location = "L1", Body = "B1", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 2, Name = "Zone 2", Location = "L2", Body = "B2", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 3, Name = "Zone 3", Location = "L3", Body = "B3", Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.Zones.Should().HaveCount(2);
            _context.Zones.Find(2).Should().BeNull();
            _context.Zones.Find(1).Should().NotBeNull();
            _context.Zones.Find(3).Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Zone_FullLifecycle()
        {
            // Arrange
            var zone = new Zone
            {
                Id = 1,
                Name = "Lifecycle Zone",
                Location = "Test",
                Body = "<p>Initial content</p>",
                Visible = false,
                Timestamp = DateTime.UtcNow
            };

            // Act - Add
            _service.Add(zone);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Zone";
            added.Body = "<p>Updated content</p>";
            added.Visible = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Name.Should().Be("Updated Lifecycle Zone");
            updated.Visible.Should().BeTrue();

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManagePageLayout_WithMultipleZones()
        {
            // Arrange - Create zones for a typical page layout
            var zones = new[]
            {
                new Zone { Id = 1, Name = "Header", Location = "Top", Body = "<header>Site Header</header>", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 2, Name = "Sidebar Left", Location = "Left", Body = "<aside>Left sidebar</aside>", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 3, Name = "Main Content", Location = "Center", Body = "<main>Main content area</main>", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 4, Name = "Sidebar Right", Location = "Right", Body = "<aside>Right sidebar</aside>", Visible = true, Timestamp = DateTime.UtcNow },
                new Zone { Id = 5, Name = "Footer", Location = "Bottom", Body = "<footer>Site Footer</footer>", Visible = true, Timestamp = DateTime.UtcNow }
            };

            // Act
            foreach (var zone in zones)
            {
                _service.Add(zone);
            }

            var allZones = _service.GetAll();
            var topZones = allZones.Where(z => z.Location == "Top").ToList();
            var bottomZones = allZones.Where(z => z.Location == "Bottom").ToList();

            // Assert
            allZones.Should().HaveCount(5);
            topZones.Should().HaveCount(1);
            bottomZones.Should().HaveCount(1);
        }

        [Test]
        public void ZoneVisibility_CanBeControlled()
        {
            // Arrange
            _service.Add(new Zone { Id = 1, Name = "Visible Zone", Location = "Vis", Body = "Content", Visible = true, Timestamp = DateTime.UtcNow });
            _service.Add(new Zone { Id = 2, Name = "Hidden Zone", Location = "Hid", Body = "Content", Visible = false, Timestamp = DateTime.UtcNow });

            // Act
            var allZones = _service.GetAll();
            var visible = allZones.Where(z => z.Visible).ToList();
            var hidden = allZones.Where(z => !z.Visible).ToList();

            // Assert
            visible.Should().HaveCount(1);
            hidden.Should().HaveCount(1);
        }

        [Test]
        public void ZoneContent_CanContainComplexHtml()
        {
            // Arrange
            var complexHtml = @"
                <div class='widget'>
                    <h3>Widget Title</h3>
                    <ul>
                        <li><a href='#'>Link 1</a></li>
                        <li><a href='#'>Link 2</a></li>
                    </ul>
                    <script>console.log('widget loaded');</script>
                </div>";

            var zone = new Zone
            {
                Id = 1,
                Name = "Complex Widget",
                Location = "Side",
                Body = complexHtml,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(zone);
            var retrieved = _service.Get(1);

            // Assert
            retrieved!.Body.Should().Contain("Widget Title");
            retrieved.Body.Should().Contain("<script>");
            retrieved.Body.Should().Contain("Link 1");
        }

        [Test]
        public void MultipleZones_CanShareLocation()
        {
            // Arrange
            _service.Add(new Zone { Id = 1, Name = "Widget 1", Location = "Sidebar", Body = "Widget 1 content", Visible = true, Timestamp = DateTime.UtcNow });
            _service.Add(new Zone { Id = 2, Name = "Widget 2", Location = "Sidebar", Body = "Widget 2 content", Visible = true, Timestamp = DateTime.UtcNow });
            _service.Add(new Zone { Id = 3, Name = "Widget 3", Location = "Sidebar", Body = "Widget 3 content", Visible = true, Timestamp = DateTime.UtcNow });

            // Act
            var sidebarZones = _service.GetAll().Where(z => z.Location == "Sidebar").ToList();

            // Assert
            sidebarZones.Should().HaveCount(3);
        }

        #endregion
    }
}
