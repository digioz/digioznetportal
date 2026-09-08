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
    /// Unit tests for VisitorInfoService - Website visitor tracking
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    public class VisitorInfoServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private VisitorInfoService _service;

        [SetUp]
        public void Setup()
        {
            // Use In-Memory database for isolated tests
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new VisitorInfoService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsVisitorInfo()
        {
            // Arrange
            var visitor = new VisitorInfo
            {
                Id = 1,
                IpAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0",
                Href = "/home",
                Timestamp = DateTime.UtcNow
            };
            _context.VisitorInfos.Add(visitor);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.IpAddress.Should().Be("192.168.1.1");
            result.Href.Should().Be("/home");
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
        public void GetAll_WithMultipleVisitors_ReturnsAll()
        {
            // Arrange
            _context.VisitorInfos.AddRange(
                new VisitorInfo { Id = 1, IpAddress = "192.168.1.1", Href = "/home", Timestamp = DateTime.UtcNow.AddDays(-2) },
                new VisitorInfo { Id = 2, IpAddress = "192.168.1.2", Href = "/about", Timestamp = DateTime.UtcNow.AddDays(-1) },
                new VisitorInfo { Id = 3, IpAddress = "192.168.1.3", Href = "/contact", Timestamp = DateTime.UtcNow }
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

        #region GetAllGreaterThan Tests

        [Test]
        public void GetAllGreaterThan_WithTimestamp_ReturnsOnlyNewerRecords()
        {
            // Arrange
            var cutoffDate = DateTime.UtcNow;
            _context.VisitorInfos.AddRange(
                new VisitorInfo { Id = 1, IpAddress = "192.168.1.1", Href = "/home", Timestamp = cutoffDate.AddDays(-2) },
                new VisitorInfo { Id = 2, IpAddress = "192.168.1.2", Href = "/about", Timestamp = cutoffDate.AddHours(1) },
                new VisitorInfo { Id = 3, IpAddress = "192.168.1.3", Href = "/contact", Timestamp = cutoffDate.AddHours(2) }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetAllGreaterThan(cutoffDate);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(v => v.Timestamp.Should().BeAfter(cutoffDate));
        }

        #endregion

        #region GetLastN Tests

        [Test]
        public void GetLastN_WithCount_ReturnsLastNRecords()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.VisitorInfos.Add(new VisitorInfo
                {
                    Id = i,
                    IpAddress = $"192.168.1.{i}",
                    Href = $"/page{i}",
                    Timestamp = DateTime.UtcNow.AddDays(-i)
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetLastN(5, "DESC");

            // Assert
            results.Should().HaveCount(5);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidVisitorInfo_AddsToDatabase()
        {
            // Arrange
            var visitor = new VisitorInfo
            {
                IpAddress = "10.0.0.1",
                UserAgent = "Mozilla/5.0",
                Href = "/products",
                Timestamp = DateTime.UtcNow
            };

            // Act
            _service.Add(visitor);

            // Assert
            var saved = _context.VisitorInfos.FirstOrDefault(v => v.IpAddress == "10.0.0.1");
            saved.Should().NotBeNull();
            saved!.Href.Should().Be("/products");
        }

        #endregion

        #region AddRange Tests

        [Test]
        public void AddRange_WithMultipleVisitors_AddsAllToDatabase()
        {
            // Arrange
            var visitors = new List<VisitorInfo>
            {
                new VisitorInfo { IpAddress = "10.0.0.1", Href = "/home", Timestamp = DateTime.UtcNow },
                new VisitorInfo { IpAddress = "10.0.0.2", Href = "/about", Timestamp = DateTime.UtcNow },
                new VisitorInfo { IpAddress = "10.0.0.3", Href = "/contact", Timestamp = DateTime.UtcNow }
            };

            // Act
            _service.AddRange(visitors);

            // Assert
            _context.VisitorInfos.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingVisitorInfo_UpdatesInDatabase()
        {
            // Arrange
            var visitor = new VisitorInfo
            {
                Id = 1L,
                IpAddress = "192.168.1.1",
                Href = "/old-page",
                Timestamp = DateTime.UtcNow
            };
            _context.VisitorInfos.Add(visitor);
            _context.SaveChanges();

            // Act
            visitor.Href = "/new-page";
            _service.Update(visitor);

            // Assert
            var updated = _context.VisitorInfos.Find(1L);
            updated!.Href.Should().Be("/new-page");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var visitor = new VisitorInfo
            {
                Id = 1L,
                IpAddress = "192.168.1.1",
                Href = "/home",
                Timestamp = DateTime.UtcNow
            };
            _context.VisitorInfos.Add(visitor);
            _context.SaveChanges();

            // Act
            _service.Delete(1L);

            // Assert
            var deleted = _context.VisitorInfos.Find(1L);
            deleted.Should().BeNull();
        }

        [Test]
        public void Delete_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.Delete(999);
            act.Should().NotThrow();
        }

        #endregion

        #region GetPaged Tests

        [Test]
        public void GetPaged_WithValidPageNumber_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 20; i++)
            {
                _context.VisitorInfos.Add(new VisitorInfo
                {
                    Id = i,
                    IpAddress = $"192.168.1.{i}",
                    Href = $"/page{i}",
                    Timestamp = DateTime.UtcNow.AddDays(-i)
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetPaged(2, 5);

            // Assert
            results.Should().HaveCount(5);
        }

        #endregion

        #region SearchPaged Tests

        [Test]
        public void SearchPaged_WithSearchTerm_ReturnsMatchingRecords()
        {
            // Arrange
            _context.VisitorInfos.AddRange(
                new VisitorInfo { Id = 1, IpAddress = "192.168.1.1", Href = "/products", Timestamp = DateTime.UtcNow },
                new VisitorInfo { Id = 2, IpAddress = "192.168.1.2", Href = "/about", Timestamp = DateTime.UtcNow },
                new VisitorInfo { Id = 3, IpAddress = "192.168.1.3", Href = "/product-details", Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.SearchPaged("product", 1, 10);

            // Assert
            results.Should().HaveCount(2);
        }

        #endregion

        #region CountAll Tests

        [Test]
        public void CountAll_WithMultipleRecords_ReturnsCorrectCount()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.VisitorInfos.Add(new VisitorInfo
                {
                    Id = i,
                    IpAddress = $"192.168.1.{i}",
                    Href = "/home",
                    Timestamp = DateTime.UtcNow
                });
            }
            _context.SaveChanges();

            // Act
            var count = _service.CountAll();

            // Assert
            count.Should().Be(10);
        }

        #endregion

        #region CountSearch Tests

        [Test]
        public void CountSearch_WithSearchTerm_ReturnsMatchingCount()
        {
            // Arrange
            _context.VisitorInfos.AddRange(
                new VisitorInfo { Id = 1, IpAddress = "192.168.1.1", Href = "/admin", Timestamp = DateTime.UtcNow },
                new VisitorInfo { Id = 2, IpAddress = "192.168.1.2", Href = "/user", Timestamp = DateTime.UtcNow },
                new VisitorInfo { Id = 3, IpAddress = "192.168.1.3", Href = "/admin-panel", Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountSearch("admin");

            // Assert
            count.Should().Be(2);
        }

        #endregion

        #region GetByDateRange Tests

        [Test]
        public void GetByDateRange_WithDateRange_ReturnsRecordsInRange()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-5);
            var endDate = DateTime.UtcNow.AddDays(-2);

            _context.VisitorInfos.AddRange(
                new VisitorInfo { Id = 1, IpAddress = "192.168.1.1", Href = "/p1", Timestamp = startDate.AddDays(-1) },
                new VisitorInfo { Id = 2, IpAddress = "192.168.1.2", Href = "/p2", Timestamp = startDate.AddHours(1) },
                new VisitorInfo { Id = 3, IpAddress = "192.168.1.3", Href = "/p3", Timestamp = endDate.AddHours(-1) },
                new VisitorInfo { Id = 4, IpAddress = "192.168.1.4", Href = "/p4", Timestamp = endDate.AddDays(1) }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByDateRange(startDate, endDate);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(v => 
            {
                v.Timestamp.Should().BeOnOrAfter(startDate);
                v.Timestamp.Should().BeOnOrBefore(endDate);
            });
        }

        #endregion

        #region DeleteRange Tests

        [Test]
        public void DeleteRange_WithMultipleIds_DeletesAllSpecifiedRecords()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.VisitorInfos.Add(new VisitorInfo
                {
                    Id = i,
                    IpAddress = $"192.168.1.{i}",
                    Href = "/home",
                    Timestamp = DateTime.UtcNow
                });
            }
            _context.SaveChanges();

            var idsToDelete = new List<long> { 1, 3, 5 };

            // Act
            var deletedCount = _service.DeleteRange(idsToDelete);

            // Assert
            deletedCount.Should().Be(3);
            _context.VisitorInfos.Should().HaveCount(7);
        }

        [Test]
        public void DeleteRange_WithEmptyList_DeletesZeroRecords()
        {
            // Arrange
            _context.VisitorInfos.Add(new VisitorInfo
            {
                Id = 1,
                IpAddress = "192.168.1.1",
                Href = "/home",
                Timestamp = DateTime.UtcNow
            });
            _context.SaveChanges();

            var idsToDelete = new List<long>();

            // Act
            var deletedCount = _service.DeleteRange(idsToDelete);

            // Assert
            deletedCount.Should().Be(0);
            _context.VisitorInfos.Should().HaveCount(1);
        }

        #endregion

        #region GetUniqueVisitorCountsByDate Tests

        [Test]
        [Ignore("GetUniqueVisitorCountsByDate may use grouping or other EF operations not supported by in-memory provider")]
        public void GetUniqueVisitorCountsByDate_WithDateRange_ReturnsCountsByDate()
        {
            // Note: This method uses advanced EF Core operations that may not be supported by in-memory database.
            // Real integration tests should verify this functionality with a real database.
        }

        #endregion
    }
}
