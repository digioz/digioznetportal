using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for PictureService - Photo management including upload, filtering, and searching
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Pictures")]
    public class PictureServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private PictureService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new PictureService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsPicture()
        {
            // Arrange
            var picture = new Picture
            {
                Id = 1,
                UserId = "user-1",
                AlbumId = 1,
                Filename = "photo.jpg",
                Description = "Beautiful sunset",
                Approved = true,
                Visible = true,
                Thumbnail = "thumb.jpg",
                Timestamp = DateTime.UtcNow,
                Views = 10
            };
            _context.Pictures.Add(picture);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Filename.Should().Be("photo.jpg");
            result.Views.Should().Be(10);
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
        public void GetAll_WithMultiplePictures_ReturnsAll()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "Pic 1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "Pic 2", Approved = true, Visible = false, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 2, Filename = "pic3.jpg", Description = "Pic 3", Approved = false, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
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

        #region GetByIds Tests

        [Test]
        public void GetByIds_WithValidIds_ReturnsPictures()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByIds(new List<int> { 1, 3 });

            // Assert
            results.Should().HaveCount(2);
            results.Select(p => p.Id).Should().Contain(new[] { 1, 3 });
        }

        [Test]
        public void GetByIds_WithEmptyList_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByIds(new List<int>());

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByIds_WithNonExistingIds_ReturnsEmpty()
        {
            // Arrange
            _context.Pictures.Add(new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 });
            _context.SaveChanges();

            // Act
            var results = _service.GetByIds(new List<int> { 999, 1000 });

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region GetFiltered Tests

        [Test]
        public void GetFiltered_WithNoCriteria_ReturnsOnlyVisibleApproved()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = false, Visible = false, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act - Without userId and not admin, should only see visible AND approved pictures
            var results = _service.GetFiltered();

            // Assert - Only visible and approved pictures (not user's unapproved ones)
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(p => p.Visible.Should().BeTrue());
            results.Should().AllSatisfy(p => p.Approved.Should().BeTrue());
        }

        [Test]
        public void GetFiltered_ByUserId_FiltersCorrectly()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act - Filter by user-1 (user can see their own pictures OR public approved ones)
            var results = _service.GetFiltered(userId: "user-1");

            // Assert - Should return user-1's pictures plus visible approved pictures from others
            results.Should().HaveCount(3);
            var user1Pics = results.Where(p => p.UserId == "user-1");
            user1Pics.Should().HaveCount(2);
        }

        [Test]
        public void GetFiltered_ByAlbumId_FiltersCorrectly()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 2, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetFiltered(albumId: 1);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(p => p.AlbumId.Should().Be(1));
        }

        [Test]
        public void GetFiltered_ByVisible_FiltersCorrectly()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = false, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetFiltered(visible: true);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(p => p.Visible.Should().BeTrue());
        }

        [Test]
        public void GetFiltered_ByApproved_FiltersCorrectly()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = false, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetFiltered(approved: true);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(p => p.Approved.Should().BeTrue());
        }

        [Test]
        public void GetFiltered_AdminCanSeePending()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = false, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var nonAdminResults = _service.GetFiltered(isAdmin: false);
            var adminResults = _service.GetFiltered(isAdmin: true);

            // Assert
            nonAdminResults.Should().HaveCount(1);
            adminResults.Should().HaveCount(2);
        }

        #endregion

        #region CountByUserId Tests

        [Test]
        public void CountByUserId_WithValidUserId_ReturnsCorrectCount()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountByUserId("user-1");

            // Assert
            count.Should().Be(2);
        }

        [Test]
        public void CountByUserId_WithNonExistingUserId_ReturnsZero()
        {
            // Act
            var count = _service.CountByUserId("nonexistent-user");

            // Assert
            count.Should().Be(0);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidPicture_AddsToDatabase()
        {
            // Arrange
            var picture = new Picture
            {
                Id = 1,
                UserId = "user-1",
                AlbumId = 1,
                Filename = "photo.jpg",
                Description = "Test photo",
                Approved = true,
                Visible = true,
                Thumbnail = "thumb.jpg",
                Timestamp = DateTime.UtcNow,
                Views = 0
            };

            // Act
            _service.Add(picture);

            // Assert
            var saved = _context.Pictures.Find(1);
            saved.Should().NotBeNull();
            saved!.Filename.Should().Be("photo.jpg");
        }

        [Test]
        public void Add_MultiplePictures_AllAreSaved()
        {
            // Arrange
            var pictures = new[]
            {
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = false, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            };

            // Act
            foreach (var picture in pictures)
            {
                _service.Add(picture);
            }

            // Assert
            _context.Pictures.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingPicture_UpdatesInDatabase()
        {
            // Arrange
            var picture = new Picture
            {
                Id = 1,
                UserId = "user-1",
                AlbumId = 1,
                Filename = "original.jpg",
                Description = "Original",
                Approved = false,
                Visible = false,
                Timestamp = DateTime.UtcNow,
                Views = 5
            };
            _context.Pictures.Add(picture);
            _context.SaveChanges();

            // Act
            picture.Description = "Updated description";
            picture.Approved = true;
            picture.Visible = true;
            _service.Update(picture);

            // Assert
            var updated = _context.Pictures.Find(1);
            updated!.Description.Should().Be("Updated description");
            updated.Approved.Should().BeTrue();
            updated.Visible.Should().BeTrue();
        }

        [Test]
        public void Update_DoesNotAffectOtherPictures()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            var picture1 = _context.Pictures.Find(1);
            picture1!.Description = "Updated P1";

            // Act
            _service.Update(picture1);

            // Assert
            _context.Pictures.Find(2)!.Description.Should().Be("P2");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var picture = new Picture
            {
                Id = 1,
                UserId = "user-1",
                AlbumId = 1,
                Filename = "temp.jpg",
                Description = "Temporary",
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow,
                Views = 0
            };
            _context.Pictures.Add(picture);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Pictures.Find(1);
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
        public void Delete_RemovesCorrectPictureWhenMultipleExist()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            _service.Delete(2);

            // Assert
            _context.Pictures.Should().HaveCount(2);
            _context.Pictures.Find(2).Should().BeNull();
        }

        #endregion

        #region IncrementViews Tests

        [Test]
        [Ignore("EF Core InMemory does not support ExecuteUpdate operations")]
        public void IncrementViews_WithValidId_IncrementsViewCount()
        {
            // Arrange
            var picture = new Picture
            {
                Id = 1,
                UserId = "user-1",
                AlbumId = 1,
                Filename = "photo.jpg",
                Description = "Test",
                Approved = true,
                Visible = true,
                Timestamp = DateTime.UtcNow,
                Views = 5
            };
            _context.Pictures.Add(picture);
            _context.SaveChanges();

            // Act
            _service.IncrementViews(1);

            // Assert
            var updated = _context.Pictures.Find(1);
            updated!.Views.Should().Be(6);
        }

        #endregion

        #region DeleteByUserId Tests

        [Test]
        [Ignore("EF Core InMemory does not support ExecuteDelete operations")]
        public void DeleteByUserId_WithValidUserId_RemovesAllUserPictures()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var deleted = _service.DeleteByUserId("user-1");

            // Assert
            deleted.Should().Be(2);
            _context.Pictures.Should().HaveCount(1);
        }

        #endregion

        #region ReassignByUserId Tests

        [Test]
        [Ignore("EF Core InMemory does not support ExecuteUpdate operations")]
        public void ReassignByUserId_WithValidUserIds_ReassignsPictures()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var reassigned = _service.ReassignByUserId("user-1", "user-3");

            // Assert
            reassigned.Should().Be(2);
            var updated = _context.Pictures.Where(p => p.UserId == "user-3").ToList();
            updated.Should().HaveCount(2);
        }

        #endregion

        #region Search Tests

        [Test]
        public void Search_ByTerm_FindsMatchingFilename()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "sunset.jpg", Description = "A sunset scene", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "mountain.jpg", Description = "Mountain range", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-2", AlbumId = 1, Filename = "sunset_beach.jpg", Description = "Beach sunset", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("sunset", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(2);
            totalCount.Should().Be(2);
        }

        [Test]
        public void Search_ByTerm_FindsMatchingDescription()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "photo1.jpg", Description = "Beautiful sunset", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "photo2.jpg", Description = "Mountain landscape", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("sunset", 0, 10, out int totalCount);

            // Assert
            results.Should().HaveCount(1);
            results.First().Description.Should().Contain("sunset");
        }

        [Test]
        public void Search_WithNoMatches_ReturnsEmpty()
        {
            // Arrange
            _context.Pictures.Add(new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "photo.jpg", Description = "A photo", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 });
            _context.SaveChanges();

            // Act
            var results = _service.Search("nonexistent", 0, 10, out int totalCount);

            // Assert
            results.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Test]
        public void Search_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.Pictures.Add(new Picture
                {
                    Id = i,
                    UserId = "user-1",
                    AlbumId = 1,
                    Filename = $"photo{i}.jpg",
                    Description = $"Photo {i} with keyword",
                    Approved = true,
                    Visible = true,
                    Timestamp = DateTime.UtcNow,
                    Views = 0
                });
            }
            _context.SaveChanges();

            // Act
            var page1 = _service.Search("keyword", 0, 5, out int totalCount1);
            var page2 = _service.Search("keyword", 5, 5, out int totalCount2);

            // Assert
            page1.Should().HaveCount(5);
            page2.Should().HaveCount(5);
            totalCount1.Should().Be(10);
            totalCount2.Should().Be(10);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Picture_FullLifecycle()
        {
            // Arrange
            var picture = new Picture
            {
                Id = 1,
                UserId = "user-1",
                AlbumId = 1,
                Filename = "lifecycle.jpg",
                Description = "Lifecycle test",
                Approved = false,
                Visible = false,
                Timestamp = DateTime.UtcNow,
                Views = 0
            };

            // Act - Add
            _service.Add(picture);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Description = "Updated description";
            added.Approved = true;
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Description.Should().Be("Updated description");
            updated.Approved.Should().BeTrue();

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void UserCanUploadMultiplePictures()
        {
            // Arrange
            var userId = "user-1";
            var pictureCount = 5;

            // Act
            for (int i = 1; i <= pictureCount; i++)
            {
                _service.Add(new Picture
                {
                    Id = i,
                    UserId = userId,
                    AlbumId = 1,
                    Filename = $"photo{i}.jpg",
                    Description = $"Photo {i}",
                    Approved = true,
                    Visible = true,
                    Timestamp = DateTime.UtcNow,
                    Views = 0
                });
            }

            var count = _service.CountByUserId(userId);
            var userPictures = _service.GetFiltered(userId: userId);

            // Assert
            count.Should().Be(pictureCount);
            userPictures.Should().HaveCount(pictureCount);
        }

        [Test]
        public void FilteredPicturesRespectApprovalState()
        {
            // Arrange
            _context.Pictures.AddRange(
                new Picture { Id = 1, UserId = "user-1", AlbumId = 1, Filename = "pic1.jpg", Description = "P1", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 2, UserId = "user-1", AlbumId = 1, Filename = "pic2.jpg", Description = "P2", Approved = false, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 },
                new Picture { Id = 3, UserId = "user-1", AlbumId = 1, Filename = "pic3.jpg", Description = "P3", Approved = true, Visible = true, Timestamp = DateTime.UtcNow, Views = 0 }
            );
            _context.SaveChanges();

            // Act
            var approved = _service.GetFiltered(approved: true, isAdmin: false);
            var allForAdmin = _service.GetFiltered(isAdmin: true);

            // Assert
            approved.Should().HaveCount(2);
            allForAdmin.Should().HaveCount(3);
        }

        #endregion
    }
}
