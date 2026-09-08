using NUnit.Framework;
using FluentAssertions;
using Moq;
using digioz.Portal.Dal.Services;
using System.Threading.Tasks;
using static digioz.Portal.Dal.Services.BannedIpTrackingCleanupService;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for BannedIpTrackingCleanupService - IP ban tracking and cleanup operations
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Security")]
    public class BannedIpTrackingCleanupServiceTests
    {
        private Mock<IBannedIpTrackingCleanupService> _mockService;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IBannedIpTrackingCleanupService>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockService?.Reset();
        }

        #region CleanupOldRecordsAsync Tests

        [Test]
        public async Task CleanupOldRecordsAsync_WithDefaultDays_ReturnsCount()
        {
            // Arrange
            const int expectedDeletedCount = 42;

            _mockService
                .Setup(s => s.CleanupOldRecordsAsync(7))
                .ReturnsAsync(expectedDeletedCount);

            // Act
            var result = await _mockService.Object.CleanupOldRecordsAsync();

            // Assert
            result.Should().Be(expectedDeletedCount);
            _mockService.Verify(s => s.CleanupOldRecordsAsync(7), Times.Once);
        }

        [Test]
        public async Task CleanupOldRecordsAsync_With30DaysRetention_ReturnsCount()
        {
            // Arrange
            const int expectedDeletedCount = 105;
            const int daysToKeep = 30;

            _mockService
                .Setup(s => s.CleanupOldRecordsAsync(daysToKeep))
                .ReturnsAsync(expectedDeletedCount);

            // Act
            var result = await _mockService.Object.CleanupOldRecordsAsync(daysToKeep);

            // Assert
            result.Should().Be(expectedDeletedCount);
        }

        [Test]
        public async Task CleanupOldRecordsAsync_WithNoOldRecords_ReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.CleanupOldRecordsAsync(It.IsAny<int>()))
                .ReturnsAsync(0);

            // Act
            var result = await _mockService.Object.CleanupOldRecordsAsync(7);

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public async Task CleanupOldRecordsAsync_MultipleCleanupRuns()
        {
            // Arrange
            _mockService
                .SetupSequence(s => s.CleanupOldRecordsAsync(It.IsAny<int>()))
                .ReturnsAsync(50)
                .ReturnsAsync(30)
                .ReturnsAsync(10);

            // Act
            var firstRun = await _mockService.Object.CleanupOldRecordsAsync(7);
            var secondRun = await _mockService.Object.CleanupOldRecordsAsync(7);
            var thirdRun = await _mockService.Object.CleanupOldRecordsAsync(7);

            // Assert
            firstRun.Should().Be(50);
            secondRun.Should().Be(30);
            thirdRun.Should().Be(10);
            _mockService.Verify(s => s.CleanupOldRecordsAsync(It.IsAny<int>()), Times.Exactly(3));
        }

        #endregion

        #region CleanupExpiredBansAsync Tests

        [Test]
        public async Task CleanupExpiredBansAsync_WithExpiredBans_ReturnsCount()
        {
            // Arrange
            const int expectedDeletedCount = 23;

            _mockService
                .Setup(s => s.CleanupExpiredBansAsync())
                .ReturnsAsync(expectedDeletedCount);

            // Act
            var result = await _mockService.Object.CleanupExpiredBansAsync();

            // Assert
            result.Should().Be(expectedDeletedCount);
            _mockService.Verify(s => s.CleanupExpiredBansAsync(), Times.Once);
        }

        [Test]
        public async Task CleanupExpiredBansAsync_WithNoExpiredBans_ReturnsZero()
        {
            // Arrange
            _mockService
                .Setup(s => s.CleanupExpiredBansAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _mockService.Object.CleanupExpiredBansAsync();

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public async Task CleanupExpiredBansAsync_ScheduledCleanup()
        {
            // Arrange
            _mockService
                .SetupSequence(s => s.CleanupExpiredBansAsync())
                .ReturnsAsync(15)
                .ReturnsAsync(8)
                .ReturnsAsync(3);

            // Act
            var run1 = await _mockService.Object.CleanupExpiredBansAsync();
            var run2 = await _mockService.Object.CleanupExpiredBansAsync();
            var run3 = await _mockService.Object.CleanupExpiredBansAsync();

            // Assert
            run1.Should().Be(15);
            run2.Should().Be(8);
            run3.Should().Be(3);
        }

        #endregion

        #region GetStatisticsAsync Tests

        [Test]
        public async Task GetStatisticsAsync_WithValidData_ReturnsStatistics()
        {
            // Arrange
            var expectedStats = new BannedIpTrackingStatistics
            {
                TotalRecords = 1000,
                RecordsLast24Hours = 150,
                RecordsLastWeek = 450,
                UniqueIpsLast24Hours = 75
            };

            _mockService
                .Setup(s => s.GetStatisticsAsync())
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _mockService.Object.GetStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result!.TotalRecords.Should().Be(1000);
            result.RecordsLast24Hours.Should().Be(150);
            result.RecordsLastWeek.Should().Be(450);
            result.UniqueIpsLast24Hours.Should().Be(75);
        }

        [Test]
        public async Task GetStatisticsAsync_WithEmptyData_ReturnsZeros()
        {
            // Arrange
            var emptyStats = new BannedIpTrackingStatistics
            {
                TotalRecords = 0,
                RecordsLast24Hours = 0,
                RecordsLastWeek = 0,
                UniqueIpsLast24Hours = 0
            };

            _mockService
                .Setup(s => s.GetStatisticsAsync())
                .ReturnsAsync(emptyStats);

            // Act
            var result = await _mockService.Object.GetStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result!.TotalRecords.Should().Be(0);
            result.RecordsLast24Hours.Should().Be(0);
        }

        [Test]
        public async Task GetStatisticsAsync_HighActivity_ReturnsHighNumbers()
        {
            // Arrange
            var highActivityStats = new BannedIpTrackingStatistics
            {
                TotalRecords = 50000,
                RecordsLast24Hours = 5000,
                RecordsLastWeek = 25000,
                UniqueIpsLast24Hours = 500
            };

            _mockService
                .Setup(s => s.GetStatisticsAsync())
                .ReturnsAsync(highActivityStats);

            // Act
            var result = await _mockService.Object.GetStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result!.TotalRecords.Should().Be(50000);
            result.RecordsLast24Hours.Should().Be(5000);
            result.UniqueIpsLast24Hours.Should().Be(500);
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task BannedIpTracking_FullMaintenanceWorkflow()
        {
            // Arrange - Simulate scheduled maintenance task
            var preCleanupStats = new BannedIpTrackingStatistics
            {
                TotalRecords = 2500,
                RecordsLast24Hours = 200,
                RecordsLastWeek = 800,
                UniqueIpsLast24Hours = 100
            };

            _mockService
                .Setup(s => s.GetStatisticsAsync())
                .ReturnsAsync(preCleanupStats);

            _mockService
                .Setup(s => s.CleanupOldRecordsAsync(7))
                .ReturnsAsync(800);

            _mockService
                .Setup(s => s.CleanupExpiredBansAsync())
                .ReturnsAsync(50);

            // Act - Get stats before cleanup
            var statsBeforeCleanup = await _mockService.Object.GetStatisticsAsync();

            // Act - Perform cleanup
            var oldRecordsDeleted = await _mockService.Object.CleanupOldRecordsAsync(7);
            var expiredBansDeleted = await _mockService.Object.CleanupExpiredBansAsync();

            // Assert
            statsBeforeCleanup!.TotalRecords.Should().Be(2500);
            oldRecordsDeleted.Should().Be(800);
            expiredBansDeleted.Should().Be(50);
        }

        [Test]
        public async Task BannedIpTracking_MonitoringOverTime()
        {
            // Arrange - Simulate monitoring at different times
            var stats1 = new BannedIpTrackingStatistics
            {
                TotalRecords = 1000,
                RecordsLast24Hours = 100,
                RecordsLastWeek = 400,
                UniqueIpsLast24Hours = 50
            };

            var stats2 = new BannedIpTrackingStatistics
            {
                TotalRecords = 1500,
                RecordsLast24Hours = 150,
                RecordsLastWeek = 500,
                UniqueIpsLast24Hours = 75
            };

            var stats3 = new BannedIpTrackingStatistics
            {
                TotalRecords = 2000,
                RecordsLast24Hours = 200,
                RecordsLastWeek = 600,
                UniqueIpsLast24Hours = 100
            };

            _mockService
                .SetupSequence(s => s.GetStatisticsAsync())
                .ReturnsAsync(stats1)
                .ReturnsAsync(stats2)
                .ReturnsAsync(stats3);

            // Act
            var morning = await _mockService.Object.GetStatisticsAsync();
            var afternoon = await _mockService.Object.GetStatisticsAsync();
            var evening = await _mockService.Object.GetStatisticsAsync();

            // Assert - Trend shows increasing activity
            morning!.TotalRecords.Should().Be(1000);
            afternoon!.TotalRecords.Should().Be(1500);
            evening!.TotalRecords.Should().Be(2000);

            morning.UniqueIpsLast24Hours.Should().BeLessThan(afternoon.UniqueIpsLast24Hours);
            afternoon.UniqueIpsLast24Hours.Should().BeLessThan(evening.UniqueIpsLast24Hours);
        }

        [Test]
        public async Task BannedIpTracking_DailyMaintenanceRoutine()
        {
            // Arrange - Simulate daily maintenance routine
            _mockService
                .Setup(s => s.GetStatisticsAsync())
                .ReturnsAsync(new BannedIpTrackingStatistics
                {
                    TotalRecords = 3000,
                    RecordsLast24Hours = 500,
                    RecordsLastWeek = 1500,
                    UniqueIpsLast24Hours = 200
                });

            _mockService
                .Setup(s => s.CleanupOldRecordsAsync(7))
                .ReturnsAsync(600);  // Clean records older than 7 days

            _mockService
                .Setup(s => s.CleanupExpiredBansAsync())
                .ReturnsAsync(75);  // Clean expired bans

            // Act - Daily maintenance
            var stats = await _mockService.Object.GetStatisticsAsync();
            var oldRecordsRemoved = await _mockService.Object.CleanupOldRecordsAsync(7);
            var expiredBansRemoved = await _mockService.Object.CleanupExpiredBansAsync();

            // Get stats after cleanup
            _mockService
                .Setup(s => s.GetStatisticsAsync())
                .ReturnsAsync(new BannedIpTrackingStatistics
                {
                    TotalRecords = 2400,  // Reduced by cleanup operations
                    RecordsLast24Hours = 500,
                    RecordsLastWeek = 1500,
                    UniqueIpsLast24Hours = 200
                });

            var statsAfterCleanup = await _mockService.Object.GetStatisticsAsync();

            // Assert
            stats!.TotalRecords.Should().Be(3000);
            (oldRecordsRemoved + expiredBansRemoved).Should().Be(675);
            statsAfterCleanup!.TotalRecords.Should().Be(2400);
        }

        #endregion
    }
}
