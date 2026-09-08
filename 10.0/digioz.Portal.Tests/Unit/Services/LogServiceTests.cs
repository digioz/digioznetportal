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
    /// Unit tests for LogService - Application logging and audit trail
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Logging")]
    public class LogServiceTests
    {
        private DbContextOptions<digiozPortalContext> _options;
        private digiozPortalContext _context;
        private LogService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new digiozPortalContext(_options);
            _service = new LogService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsLog()
        {
            // Arrange
            var log = new Log
            {
                Id = 1,
                Message = "Test message",
                Level = "INFO",
                Timestamp = DateTime.UtcNow,
                Exception = null,
                LogEvent = "TestEvent"
            };
            _context.Logs.Add(log);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("Test message");
            result.Level.Should().Be("INFO");
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
        public void GetAll_WithMultipleLogs_ReturnsAll()
        {
            // Arrange
            _context.Logs.AddRange(
                new Log { Id = 1, Message = "Log 1", Level = "INFO", Timestamp = DateTime.UtcNow, LogEvent = "Event1" },
                new Log { Id = 2, Message = "Log 2", Level = "WARNING", Timestamp = DateTime.UtcNow, LogEvent = "Event2" },
                new Log { Id = 3, Message = "Log 3", Level = "ERROR", Timestamp = DateTime.UtcNow, LogEvent = "Event3" }
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

        #region GetLastN Tests

        [Test]
        public void GetLastN_WithOrder_ReturnsLastNLogs()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.Logs.Add(new Log
                {
                    Id = i,
                    Message = $"Log {i}",
                    Level = "INFO",
                    Timestamp = DateTime.UtcNow.AddSeconds(i),
                    LogEvent = "Test"
                });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetLastN(5, "DESC");

            // Assert
            results.Should().HaveCount(5);
        }

        [Test]
        public void GetLastN_WithNGreaterThanTotal_ReturnsAll()
        {
            // Arrange
            for (int i = 1; i <= 3; i++)
            {
                _context.Logs.Add(new Log { Id = i, Message = $"Log {i}", Level = "INFO", Timestamp = DateTime.UtcNow, LogEvent = "Test" });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetLastN(10, "DESC");

            // Assert
            results.Should().HaveCount(3);
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidLog_AddsToDatabase()
        {
            // Arrange
            var log = new Log
            {
                Id = 1,
                Message = "New log entry",
                Level = "INFO",
                Timestamp = DateTime.UtcNow,
                Exception = null,
                LogEvent = "UserLogin"
            };

            // Act
            _service.Add(log);

            // Assert
            var saved = _context.Logs.Find(1);
            saved.Should().NotBeNull();
            saved!.Message.Should().Be("New log entry");
        }

        [Test]
        public void Add_WithException_SavesExceptionStackTrace()
        {
            // Arrange
            var log = new Log
            {
                Id = 1,
                Message = "Error occurred",
                Level = "ERROR",
                Timestamp = DateTime.UtcNow,
                Exception = "System.NullReferenceException: Object reference not set...",
                LogEvent = "ApplicationError"
            };

            // Act
            _service.Add(log);

            // Assert
            var saved = _context.Logs.Find(1);
            saved!.Exception.Should().Contain("NullReferenceException");
        }

        [Test]
        public void AddRange_WithMultipleLogs_AddsBatch()
        {
            // Arrange
            var logs = new List<Log>
            {
                new Log { Id = 1, Message = "Log 1", Level = "INFO", Timestamp = DateTime.UtcNow },
                new Log { Id = 2, Message = "Log 2", Level = "WARNING", Timestamp = DateTime.UtcNow },
                new Log { Id = 3, Message = "Log 3", Level = "ERROR", Timestamp = DateTime.UtcNow }
            };

            // Act
            _service.AddRange(logs);

            // Assert
            _context.Logs.Should().HaveCount(3);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingLog_UpdatesInDatabase()
        {
            // Arrange
            var log = new Log
            {
                Id = 1,
                Message = "Original message",
                Level = "INFO",
                Timestamp = DateTime.UtcNow,
                LogEvent = "Original"
            };
            _context.Logs.Add(log);
            _context.SaveChanges();

            // Act
            log.Message = "Updated message";
            log.Level = "WARNING";
            _service.Update(log);

            // Assert
            var updated = _context.Logs.Find(1);
            updated!.Message.Should().Be("Updated message");
            updated.Level.Should().Be("WARNING");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var log = new Log { Id = 1, Message = "Delete me", Level = "INFO", Timestamp = DateTime.UtcNow };
            _context.Logs.Add(log);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Logs.Find(1);
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

        #region GetLatest Tests

        [Test]
        public void GetLatest_WithCount_ReturnsLatestLogs()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                _context.Logs.Add(new Log { Id = i, Message = $"Log {i}", Level = "INFO", Timestamp = DateTime.UtcNow.AddSeconds(i) });
            }
            _context.SaveChanges();

            // Act
            var results = _service.GetLatest(3);

            // Assert
            results.Should().HaveCount(3);
        }

        #endregion

        #region Search Tests

        [Test]
        public void Search_ByTerm_FindsMatchingLogs()
        {
            // Arrange
            _context.Logs.AddRange(
                new Log { Id = 1, Message = "Database connection error", Level = "ERROR", Timestamp = DateTime.UtcNow },
                new Log { Id = 2, Message = "User login successful", Level = "INFO", Timestamp = DateTime.UtcNow },
                new Log { Id = 3, Message = "Database timeout occurred", Level = "WARNING", Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var results = _service.Search("Database", 10);

            // Assert
            results.Should().HaveCount(2);
        }

        [Test]
        public void Search_WithNoMatches_ReturnsEmpty()
        {
            // Arrange
            _context.Logs.Add(new Log { Id = 1, Message = "Test log", Level = "INFO", Timestamp = DateTime.UtcNow });
            _context.SaveChanges();

            // Act
            var results = _service.Search("NonExistent", 10);

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Paging Tests

        [Test]
        public void GetPaged_WithValidPage_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                _context.Logs.Add(new Log { Id = i, Message = $"Log {i}", Level = "INFO", Timestamp = DateTime.UtcNow });
            }
            _context.SaveChanges();

            // Act
            var page1 = _service.GetPaged(1, 10);
            var page2 = _service.GetPaged(2, 10);

            // Assert
            page1.Should().HaveCount(10);
            page2.Should().HaveCount(5);
        }

        [Test]
        public void SearchPaged_ByTerm_ReturnsPagedResults()
        {
            // Arrange
            for (int i = 1; i <= 20; i++)
            {
                _context.Logs.Add(new Log { Id = i, Message = $"Error log {i}", Level = "ERROR", Timestamp = DateTime.UtcNow });
            }
            _context.SaveChanges();

            // Act
            var page1 = _service.SearchPaged("Error", 1, 10);
            var page2 = _service.SearchPaged("Error", 2, 10);

            // Assert
            page1.Should().HaveCount(10);
            page2.Should().HaveCount(10);
        }

        #endregion

        #region Count Tests

        [Test]
        public void CountAll_ReturnsTotal()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                _context.Logs.Add(new Log { Id = i, Message = $"Log {i}", Level = "INFO", Timestamp = DateTime.UtcNow });
            }
            _context.SaveChanges();

            // Act
            var count = _service.CountAll();

            // Assert
            count.Should().Be(5);
        }

        [Test]
        public void CountSearch_ByTerm_ReturnsMatchingCount()
        {
            // Arrange
            _context.Logs.AddRange(
                new Log { Id = 1, Message = "Warning: Low memory", Level = "WARNING", Timestamp = DateTime.UtcNow },
                new Log { Id = 2, Message = "Warning: Disk full", Level = "WARNING", Timestamp = DateTime.UtcNow },
                new Log { Id = 3, Message = "Info: Process started", Level = "INFO", Timestamp = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Act
            var count = _service.CountSearch("Warning");

            // Assert
            count.Should().Be(2);
        }

        #endregion

        #region DateRange Tests

        [Test]
        public void GetByDateRange_WithValidRange_ReturnsLogsInRange()
        {
            // Arrange
            var baseTime = DateTime.UtcNow;
            _context.Logs.AddRange(
                new Log { Id = 1, Message = "Log 1", Level = "INFO", Timestamp = baseTime.AddDays(-5) },
                new Log { Id = 2, Message = "Log 2", Level = "INFO", Timestamp = baseTime.AddDays(-2) },
                new Log { Id = 3, Message = "Log 3", Level = "INFO", Timestamp = baseTime.AddDays(0) },
                new Log { Id = 4, Message = "Log 4", Level = "INFO", Timestamp = baseTime.AddDays(2) }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByDateRange(baseTime.AddDays(-3), baseTime.AddDays(1));

            // Assert
            results.Should().HaveCount(2);
        }

        [Test]
        public void GetByDateRange_WithNullStart_ReturnsAllUpToEnd()
        {
            // Arrange
            var baseTime = DateTime.UtcNow;
            _context.Logs.AddRange(
                new Log { Id = 1, Message = "Log 1", Level = "INFO", Timestamp = baseTime.AddDays(-10) },
                new Log { Id = 2, Message = "Log 2", Level = "INFO", Timestamp = baseTime.AddDays(-2) },
                new Log { Id = 3, Message = "Log 3", Level = "INFO", Timestamp = baseTime.AddDays(5) }
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByDateRange(null, baseTime.AddDays(-1));

            // Assert
            results.Should().HaveCount(2);
        }

        #endregion

        #region DeleteRange Tests

        [Test]
        public void DeleteRange_WithIds_RemovesMultipleLogs()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                _context.Logs.Add(new Log { Id = i, Message = $"Log {i}", Level = "INFO", Timestamp = DateTime.UtcNow });
            }
            _context.SaveChanges();

            // Act
            var deleted = _service.DeleteRange(new List<int> { 1, 3, 5 });

            // Assert
            deleted.Should().Be(3);
            _context.Logs.Should().HaveCount(2);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Log_FullLifecycle()
        {
            // Arrange
            var log = new Log
            {
                Id = 1,
                Message = "Lifecycle test",
                Level = "INFO",
                Timestamp = DateTime.UtcNow,
                LogEvent = "Test"
            };

            // Act - Add
            _service.Add(log);
            var added = _service.Get(1);
            added.Should().NotBeNull();

            // Act - Update
            added!.Message = "Updated lifecycle test";
            added.Level = "WARNING";
            _service.Update(added);
            var updated = _service.Get(1);
            updated!.Level.Should().Be("WARNING");

            // Act - Delete
            _service.Delete(1);
            var deleted = _service.Get(1);
            deleted.Should().BeNull();
        }

        [Test]
        public void LoggingDifferentLevels()
        {
            // Arrange
            var levels = new[] { "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL" };

            // Act - Log at different levels
            int id = 1;
            foreach (var level in levels)
            {
                _service.Add(new Log
                {
                    Id = id++,
                    Message = $"Message at {level} level",
                    Level = level,
                    Timestamp = DateTime.UtcNow,
                    LogEvent = "MultiLevel"
                });
            }

            var allLogs = _service.GetAll();
            var errorLogs = allLogs.Where(l => l.Level == "ERROR" || l.Level == "CRITICAL").ToList();

            // Assert
            allLogs.Should().HaveCount(5);
            errorLogs.Should().HaveCount(2);
        }

        [Test]
        public void ExceptionLoggingAndRetrieval()
        {
            // Arrange
            var exceptionLog = new Log
            {
                Id = 1,
                Message = "Critical system failure",
                Level = "ERROR",
                Timestamp = DateTime.UtcNow,
                Exception = "System.DivideByZeroException: Attempted to divide by zero.\r\n   at...",
                LogEvent = "CriticalFailure"
            };

            // Act
            _service.Add(exceptionLog);
            var errors = _service.Search("system failure", 10);

            // Assert
            errors.Should().HaveCount(1);
            errors.First().Exception.Should().NotBeNull();
        }

        #endregion
    }
}
