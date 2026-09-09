using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using Microsoft.EntityFrameworkCore;
using System;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for SlideShowService - Image slideshow/carousel management
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Media")]
    public class SlideShowServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private SlideShowService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new SlideShowService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsSlideShow()
        {
            // Arrange
            var slideShow = new SlideShow
            {
                Id = "slide-1",
                Image = "/images/slide1.jpg",
                Description = "First slide",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _context.SlideShows.Add(slideShow);
            _context.SaveChanges();

            // Act
            var result = _service.Get("slide-1");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be("slide-1");
            result.Image.Should().Be("/images/slide1.jpg");
        }

        [Test]
        public void Get_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = _service.Get("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleSlideShows_ReturnsAll()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.SlideShows.AddRange(
                new SlideShow { Id = "slide-1", Image = "/img/1.jpg", Description = "Slide 1", DateCreated = now, DateModified = now },
                new SlideShow { Id = "slide-2", Image = "/img/2.jpg", Description = "Slide 2", DateCreated = now, DateModified = now },
                new SlideShow { Id = "slide-3", Image = "/img/3.jpg", Description = "Slide 3", DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().HaveCount(3);
        }

        [Test]
        public void GetAll_WithNoSlideShows_ReturnsEmpty()
        {
            // Act
            var results = _service.GetAll();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidSlideShow_AddsToDatabase()
        {
            // Arrange
            var slideShow = new SlideShow
            {
                Id = "promotion-1",
                Image = "/images/promotion.jpg",
                Description = "Special promotion",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            // Act
            _service.Add(slideShow);

            // Assert
            var saved = _context.SlideShows.Find("promotion-1");
            saved.Should().NotBeNull();
            saved!.Description.Should().Be("Special promotion");
        }

        [Test]
        public void Add_WithEmptyDescription_Saves()
        {
            // Arrange
            var slideShow = new SlideShow
            {
                Id = "blank-slide",
                Image = "/images/blank.jpg",
                Description = null,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            // Act
            _service.Add(slideShow);

            // Assert
            var saved = _context.SlideShows.Find("blank-slide");
            saved!.Description.Should().BeNull();
        }

        [Test]
        public void Add_MultipleSlideShows_AllAreSaved()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var slideShows = new[]
            {
                new SlideShow { Id = "s1", Image = "/1.jpg", Description = "D1", DateCreated = now, DateModified = now },
                new SlideShow { Id = "s2", Image = "/2.jpg", Description = "D2", DateCreated = now, DateModified = now },
                new SlideShow { Id = "s3", Image = "/3.jpg", Description = "D3", DateCreated = now, DateModified = now }
            };

            // Act
            foreach (var slide in slideShows)
            {
                _service.Add(slide);
            }

            // Assert
            _context.SlideShows.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingSlideShow_UpdatesInDatabase()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var slideShow = new SlideShow
            {
                Id = "slide-1",
                Image = "/old.jpg",
                Description = "Old description",
                DateCreated = now,
                DateModified = now
            };
            _context.SlideShows.Add(slideShow);
            _context.SaveChanges();

            // Act
            slideShow.Image = "/new.jpg";
            slideShow.Description = "New description";
            slideShow.DateModified = DateTime.UtcNow;
            _service.Update(slideShow);

            // Assert
            var updated = _context.SlideShows.Find("slide-1");
            updated!.Image.Should().Be("/new.jpg");
            updated.Description.Should().Be("New description");
        }

        [Test]
        public void Update_ChangeImage_Updates()
        {
            // Arrange
            var slide = new SlideShow
            {
                Id = "carousel-1",
                Image = "/old-image.jpg",
                Description = "Description",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _context.SlideShows.Add(slide);
            _context.SaveChanges();

            // Act
            slide.Image = "/new-image.jpg";
            _service.Update(slide);

            // Assert
            var updated = _context.SlideShows.Find("carousel-1");
            updated!.Image.Should().Be("/new-image.jpg");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var slideShow = new SlideShow
            {
                Id = "delete-me",
                Image = "/temp.jpg",
                Description = "Temporary",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _context.SlideShows.Add(slideShow);
            _context.SaveChanges();

            // Act
            _service.Delete("delete-me");

            // Assert
            var deleted = _context.SlideShows.Find("delete-me");
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete("nonexistent");
            act.Should().NotThrow();
        }

        [Test]
        public void Delete_RemovesCorrectSlideWhenMultipleExist()
        {
            // Arrange
            var now = DateTime.UtcNow;
            _context.SlideShows.AddRange(
                new SlideShow { Id = "s1", Image = "/1.jpg", Description = "D1", DateCreated = now, DateModified = now },
                new SlideShow { Id = "s2", Image = "/2.jpg", Description = "D2", DateCreated = now, DateModified = now },
                new SlideShow { Id = "s3", Image = "/3.jpg", Description = "D3", DateCreated = now, DateModified = now }
            );
            _context.SaveChanges();

            // Act
            _service.Delete("s2");

            // Assert
            _context.SlideShows.Should().HaveCount(2);
            _context.SlideShows.Find("s2").Should().BeNull();
            _context.SlideShows.Find("s1").Should().NotBeNull();
            _context.SlideShows.Find("s3").Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [Test]
        public void SlideShow_FullLifecycle()
        {
            // Arrange
            var slideShow = new SlideShow
            {
                Id = "lifecycle-slide",
                Image = "/initial.jpg",
                Description = "Initial description",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            // Act - Add
            _service.Add(slideShow);
            var added = _service.Get("lifecycle-slide");
            added.Should().NotBeNull();

            // Act - Update
            added!.Image = "/updated.jpg";
            added.Description = "Updated description";
            _service.Update(added);
            var updated = _service.Get("lifecycle-slide");
            updated!.Image.Should().Be("/updated.jpg");

            // Act - Delete
            _service.Delete("lifecycle-slide");
            var deleted = _service.Get("lifecycle-slide");
            deleted.Should().BeNull();
        }

        [Test]
        public void ManageCarousel_WithMultipleSlides()
        {
            // Arrange - Create a carousel with multiple slides
            var now = DateTime.UtcNow;
            for (int i = 1; i <= 5; i++)
            {
                _service.Add(new SlideShow
                {
                    Id = $"carousel-{i}",
                    Image = $"/carousel/{i}.jpg",
                    Description = $"Carousel slide {i}",
                    DateCreated = now,
                    DateModified = now
                });
            }

            // Act
            var allSlides = _service.GetAll();

            // Assert
            allSlides.Should().HaveCount(5);
            allSlides[0].Id.Should().StartWith("carousel-");
        }

        [Test]
        public void SlideShowTimestamps_AreTracked()
        {
            // Arrange
            var createTime = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var modifyTime = createTime.AddDays(5);

            var slide = new SlideShow
            {
                Id = "timestamped",
                Image = "/image.jpg",
                Description = "Test",
                DateCreated = createTime,
                DateModified = modifyTime
            };

            // Act
            _service.Add(slide);
            var retrieved = _service.Get("timestamped");

            // Assert
            retrieved!.DateCreated.Should().Be(createTime);
            retrieved.DateModified.Should().Be(modifyTime);
        }

        #endregion
    }
}
