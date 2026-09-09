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
    /// Unit tests for VideoAlbumService - Video album management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Videos")]
    public class VideoAlbumServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private VideoAlbumService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new VideoAlbumService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsAlbum()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Tutorials 2024",
                Description = "Collection of tutorial videos",
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.VideoAlbums.Add(album);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Tutorials 2024");
            result.Description.Should().Be("Collection of tutorial videos");
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
        public void GetAll_WithMultipleAlbums_ReturnsAll()
        {
            // Arrange
            _context.VideoAlbums.AddRange(
                new VideoAlbum { Id = 1, Name = "Album 1", Description = "Desc 1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 2, Name = "Album 2", Description = "Desc 2", Approved = true, Visible = false, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 3, Name = "Album 3", Description = "Desc 3", Approved = false, Visible = true, Timestamp = DateTime.UtcNow }
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
        public void Add_WithValidAlbum_AddsToDatabase()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 10,
                Name = "New Album",
                Description = "Test album",
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(album);

            // Assert
            var saved = _context.VideoAlbums.Find(10);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("New Album");
        }

        [Test]
        public void Add_WithNullDescription_Saves()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 11,
                Name = "Album Without Description",
                Description = null,
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(album);

            // Assert
            var saved = _context.VideoAlbums.Find(11);
            saved.Should().NotBeNull();
            saved!.Description.Should().BeNull();
        }

        [Test]
        public void Add_MultipleAlbums_AllAreSaved()
        {
            // Arrange
            var albums = new[]
            {
                new VideoAlbum { Id = 1, Name = "A1", Description = "D1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 2, Name = "A2", Description = "D2", Approved = false, Visible = true, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 3, Name = "A3", Description = "D3", Approved = true, Visible = false, Timestamp = DateTime.UtcNow }
            };

            // Act
            foreach (var album in albums)
            {
                _service.Add(album);
            }

            // Assert
            _context.VideoAlbums.Should().HaveCount(3);
        }

        [Test]
        public void Add_WithoutTimestamp_Saves()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 12,
                Name = "Album Without Timestamp",
                Description = "Test",
                Approved = true,
                Visible = true,
                Timestamp = null
            };

            // Act
            _service.Add(album);

            // Assert
            var saved = _context.VideoAlbums.Find(12);
            saved.Should().NotBeNull();
            saved!.Timestamp.Should().BeNull();
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingAlbum_UpdatesInDatabase()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Original Name",
                Description = "Original Description",
                Approved = false,
                Visible = false,
                Timestamp = DateTime.UtcNow
            };
            _context.VideoAlbums.Add(album);
            _context.SaveChanges();

            // Act
            album.Name = "Updated Name";
            album.Description = "Updated Description";
            album.Approved = true;
            album.Visible = true;
            _service.Update(album);

            // Assert
            var updated = _context.VideoAlbums.Find(1);
            updated!.Name.Should().Be("Updated Name");
            updated.Description.Should().Be("Updated Description");
            updated.Approved.Should().BeTrue();
            updated.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_ApprovalStatus_Updates()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Test Album",
                Description = "Test",
                Approved = false,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.VideoAlbums.Add(album);
            _context.SaveChanges();

            // Act
            album.Approved = true;
            _service.Update(album);

            // Assert
            var updated = _context.VideoAlbums.Find(1);
            updated!.Approved.Should().BeTrue();
        }

        [Test]
        public void Update_Visibility_Updates()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Test Album",
                Description = "Test",
                Approved = true,
                Visible = false,
                Timestamp = DateTime.UtcNow
            };
            _context.VideoAlbums.Add(album);
            _context.SaveChanges();

            // Act
            album.Visible = true;
            _service.Update(album);

            // Assert
            var updated = _context.VideoAlbums.Find(1);
            updated!.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_DoesNotAffectOtherAlbums()
        {
            // Arrange
            _context.VideoAlbums.AddRange(
                new VideoAlbum { Id = 1, Name = "Album 1", Description = "Desc 1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 2, Name = "Album 2", Description = "Desc 2", Approved = false, Visible = false, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            var album1 = _context.VideoAlbums.Find(1);
            album1!.Name = "Updated Album 1";

            // Act
            _service.Update(album1);

            // Assert
            _context.VideoAlbums.Find(2)!.Name.Should().Be("Album 2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Album to Delete",
                Description = "Temporary",
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _context.VideoAlbums.Add(album);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.VideoAlbums.Find(1);
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
        public void Delete_RemovesCorrectAlbumWhenMultipleExist()
        {
            // Arrange
            _context.VideoAlbums.AddRange(
                new VideoAlbum { Id = 1, Name = "Album 1", Description = "D1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 2, Name = "Album 2", Description = "D2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow },
                new VideoAlbum { Id = 3, Name = "Album 3", Description = "D3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.VideoAlbums.Should().HaveCount(2);
            _context.VideoAlbums.Find(2).Should().BeNull();
            _context.VideoAlbums.Find(1).Should().NotBeNull();
            _context.VideoAlbums.Find(3).Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Album_FullLifecycle()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Lifecycle Album",
                Description = "Test lifecycle",
                Approved = false,
                Visible = false,
                Timestamp = DateTime.UtcNow
            };

            // Act - Add
            _service.Add(album);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Name = "Updated Lifecycle Album";
            added.Approved = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Name.Should().Be("Updated Lifecycle Album");
            updated.Approved.Should().BeTrue();

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageMultipleAlbums()
        {
            // Arrange
            var albumNames = new[] { "Webinars 2024", "Product Demos", "Interviews", "Training" };

            // Act - Create
            int id = 1;
            foreach (var name in albumNames)
            {
                _service.Add(new VideoAlbum
                {
                    Id = id++,
                    Name = name,
                    Description = $"Album: {name}",
                    Approved = true,
                    Visible = true,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Assert - Retrieve
            var allAlbums = _service.GetAll();
            allAlbums.Should().HaveCount(4);
            allAlbums.Select(a => a.Name).Should().Contain(albumNames);
        }

        [Test]
        public void Album_ApprovalsCanBeManaged()
        {
            // Arrange
            _service.Add(new VideoAlbum { Id = 1, Name = "Album 1", Description = "D", Approved = false, Visible = true, Timestamp = DateTime.UtcNow });
            _service.Add(new VideoAlbum { Id = 2, Name = "Album 2", Description = "D", Approved = false, Visible = true, Timestamp = DateTime.UtcNow });
            _service.Add(new VideoAlbum { Id = 3, Name = "Album 3", Description = "D", Approved = true, Visible = true, Timestamp = DateTime.UtcNow });

            // Act
            var album1 = _service.Get(1);
            album1!.Approved = true;
            _service.Update(album1);

            var allAlbums = _service.GetAll();
            var approvedCount = allAlbums.Count(a => a.Approved);

            // Assert
            approvedCount.Should().Be(2);
        }

        [Test]
        public void Album_VisibilityCanBeControlled()
        {
            // Arrange
            _service.Add(new VideoAlbum { Id = 1, Name = "Public Album", Description = "D", Approved = true, Visible = true, Timestamp = DateTime.UtcNow });
            _service.Add(new VideoAlbum { Id = 2, Name = "Private Album", Description = "D", Approved = true, Visible = false, Timestamp = DateTime.UtcNow });
            _service.Add(new VideoAlbum { Id = 3, Name = "Another Public", Description = "D", Approved = true, Visible = true, Timestamp = DateTime.UtcNow });

            // Act
            var allAlbums = _service.GetAll();
            var visibleCount = allAlbums.Count(a => a.Visible);
            var hiddenCount = allAlbums.Count(a => !a.Visible);

            // Assert
            visibleCount.Should().Be(2);
            hiddenCount.Should().Be(1);
        }

        [Test]
        public void AlbumNameCanBeUpdated()
        {
            // Arrange
            var album = new VideoAlbum
            {
                Id = 1,
                Name = "Original Name",
                Description = "Description",
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow
            };
            _service.Add(album);

            // Act
            var retrieved = _service.Get(1);
            retrieved!.Name = "Renamed Album";
            _service.Update(retrieved);

            // Assert
            var updated = _service.Get(1);
            updated!.Name.Should().Be("Renamed Album");
        }

        #endregion
    }
}
