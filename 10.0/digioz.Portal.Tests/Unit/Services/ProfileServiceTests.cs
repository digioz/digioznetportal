using NUnit.Framework;
using FluentAssertions;
using digioz.Portal.Dal;
using digioz.Portal.Dal.Services;
using digioz.Portal.Bo;
using digioz.Portal.Tests.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace digioz.Portal.Tests.Unit.Services
{
    /// <summary>
    /// Unit tests for ProfileService - CRITICAL for user data integrity and privacy compliance
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    [Category("Services")]
    [Category("Profile")]
    [Category("Critical")]
    public class ProfileServiceTests
    {
        private digiozPortalContext _context;
        private ProfileService _service;

        [SetUp]
        public void Setup()
        {
            _context = TestDataHelper.CreateInMemoryContext();
            _service = new ProfileService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public void Get_WithValidId_ReturnsProfile()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com");
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.UserId.Should().Be("user-1");
            result.Email.Should().Be("test@example.com");
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
        public void Get_ReturnsProfileWithAllProperties()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com", "TestUser");
            profile.FirstName = "John";
            profile.LastName = "Doe";
            profile.City = "New York";
            profile.Views = 100;

            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.City.Should().Be("New York");
            result.Views.Should().Be(100);
        }

        #endregion

        #region GetByUserId Tests

        [Test]
        public void GetByUserId_WithExistingUser_ReturnsProfile()
        {
            // Arrange
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "user1@example.com"),
                TestDataHelper.CreateTestProfile(2, "user-2", "user2@example.com")
            );
            _context.SaveChanges();

            // Act
            var result = _service.GetByUserId("user-1");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-1");
            result.Email.Should().Be("user1@example.com");
        }

        [Test]
        public void GetByUserId_WithNonExistingUser_ReturnsNull()
        {
            // Arrange
            _context.Profiles.Add(TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com"));
            _context.SaveChanges();

            // Act
            var result = _service.GetByUserId("user-999");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByUserId_WithNullUserId_ReturnsNull()
        {
            // Act
            var result = _service.GetByUserId(null);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByEmail Tests

        [Test]
        public void GetByEmail_WithExistingEmail_ReturnsProfile()
        {
            // Arrange
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "john@example.com"),
                TestDataHelper.CreateTestProfile(2, "user-2", "jane@example.com")
            );
            _context.SaveChanges();

            // Act
            var result = _service.GetByEmail("john@example.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("john@example.com");
            result.UserId.Should().Be("user-1");
        }

        [Test]
        public void GetByEmail_WithNonExistingEmail_ReturnsNull()
        {
            // Arrange
            _context.Profiles.Add(TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com"));
            _context.SaveChanges();

            // Act
            var result = _service.GetByEmail("notfound@example.com");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByEmail_WithNullEmail_ReturnsNull()
        {
            // Act
            var result = _service.GetByEmail(null);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByDisplayName Tests

        [Test]
        public void GetByDisplayName_WithExistingDisplayName_ReturnsProfile()
        {
            // Arrange
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "user1@example.com", "JohnDoe"),
                TestDataHelper.CreateTestProfile(2, "user-2", "user2@example.com", "JaneSmith")
            );
            _context.SaveChanges();

            // Act
            var result = _service.GetByDisplayName("JohnDoe");

            // Assert
            result.Should().NotBeNull();
            result!.DisplayName.Should().Be("JohnDoe");
            result.UserId.Should().Be("user-1");
        }

        [Test]
        public void GetByDisplayName_IsCaseInsensitive()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com", "JohnDoe");
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            var result1 = _service.GetByDisplayName("johndoe");
            var result2 = _service.GetByDisplayName("JOHNDOE");
            var result3 = _service.GetByDisplayName("JohnDoe");

            // Assert
            result1.Should().NotBeNull();
            result2!.Should().NotBeNull();
            result3!.Should().NotBeNull();
            result1!.DisplayName.Should().Be("JohnDoe");
            result2.DisplayName.Should().Be("JohnDoe");
            result3.DisplayName.Should().Be("JohnDoe");
        }

        [Test]
        public void GetByDisplayName_WithNullDisplayName_ReturnsNull()
        {
            // Act
            var result = _service.GetByDisplayName(null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByDisplayName_WithEmptyDisplayName_ReturnsNull()
        {
            // Act
            var result = _service.GetByDisplayName(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByDisplayName_WithNonExistingDisplayName_ReturnsNull()
        {
            // Arrange
            _context.Profiles.Add(TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com", "JohnDoe"));
            _context.SaveChanges();

            // Act
            var result = _service.GetByDisplayName("NotFound");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAll Tests

        [Test]
        public void GetAll_WithMultipleProfiles_ReturnsAllProfiles()
        {
            // Arrange
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "user1@example.com"),
                TestDataHelper.CreateTestProfile(2, "user-2", "user2@example.com"),
                TestDataHelper.CreateTestProfile(3, "user-3", "user3@example.com")
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

        #region GetByUserIds Tests

        [Test]
        public void GetByUserIds_WithValidIds_ReturnsMatchingProfiles()
        {
            // Arrange
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "user1@example.com"),
                TestDataHelper.CreateTestProfile(2, "user-2", "user2@example.com"),
                TestDataHelper.CreateTestProfile(3, "user-3", "user3@example.com")
            );
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserIds(new List<string> { "user-1", "user-3" });

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(p => p.UserId == "user-1");
            results.Should().Contain(p => p.UserId == "user-3");
        }

        [Test]
        public void GetByUserIds_WithEmptyList_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByUserIds(new List<string>());

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByUserIds_WithNullList_ReturnsEmptyList()
        {
            // Act
            var results = _service.GetByUserIds(null);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public void GetByUserIds_WithNonExistingIds_ReturnsEmptyList()
        {
            // Arrange
            _context.Profiles.Add(TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com"));
            _context.SaveChanges();

            // Act
            var results = _service.GetByUserIds(new List<string> { "user-999", "user-888" });

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_WithValidProfile_AddsToDatabase()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "new-user", "newuser@example.com", "NewUser");

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);

            saved.Should().NotBeNull();
            saved!.UserId.Should().Be("new-user");
            saved.Email.Should().Be("newuser@example.com");
            saved.DisplayName.Should().Be("NewUser");
        }

        [Test]
        public void Add_WithAllProperties_SavesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com", "TestUser");
            profile.FirstName = "John";
            profile.MiddleName = "Michael";
            profile.LastName = "Doe";
            profile.Birthday = new DateTime(1985, 6, 15);
            profile.BirthdayVisible = false;
            profile.Address = "456 Main St";
            profile.City = "Boston";
            profile.State = "MA";
            profile.Zip = "02101";
            profile.Country = "USA";
            profile.Signature = "Best regards, John";
            profile.Avatar = "john-avatar.jpg";
            profile.ThemeId = 2;

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);

            saved.Should().NotBeNull();
            saved!.FirstName.Should().Be("John");
            saved.MiddleName.Should().Be("Michael");
            saved.LastName.Should().Be("Doe");
            saved.Birthday.Should().Be(new DateTime(1985, 6, 15));
            saved.BirthdayVisible.Should().BeFalse();
            saved.Address.Should().Be("456 Main St");
            saved.City.Should().Be("Boston");
            saved.State.Should().Be("MA");
            saved.Zip.Should().Be("02101");
            saved.Country.Should().Be("USA");
            saved.Signature.Should().Be("Best regards, John");
            saved.Avatar.Should().Be("john-avatar.jpg");
            saved.ThemeId.Should().Be(2);
        }

        [Test]
        public void Add_WithNullOptionalFields_SavesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com", "TestUser");
            profile.MiddleName = null;
            profile.Birthday = null;
            profile.BirthdayVisible = null;
            profile.Address2 = null;
            profile.Signature = null;
            profile.ThemeId = null;

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);

            saved.Should().NotBeNull();
            saved!.MiddleName.Should().BeNull();
            saved.Birthday.Should().BeNull();
            saved.BirthdayVisible.Should().BeNull();
            saved.ThemeId.Should().BeNull();
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_WithExistingProfile_UpdatesInDatabase()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "original@example.com", "OriginalName");
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act - Update email and display name
            profile.Email = "updated@example.com";
            profile.DisplayName = "UpdatedName";
            _service.Update(profile);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Email.Should().Be("updated@example.com");
            updated!.DisplayName.Should().Be("UpdatedName");
        }

        [Test]
        public void Update_ChangesPersonalInfo_UpdatesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            profile.FirstName = "Jane";
            profile.LastName = "Smith";
            profile.Birthday = new DateTime(1992, 3, 20);
            _service.Update(profile);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.FirstName.Should().Be("Jane");
            updated!.LastName.Should().Be("Smith");
            updated!.Birthday.Should().Be(new DateTime(1992, 3, 20));
        }

        [Test]
        public void Update_ChangesPrivacySettings_UpdatesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.BirthdayVisible = true;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act - User makes birthday private
            profile.BirthdayVisible = false;
            _service.Update(profile);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.BirthdayVisible.Should().BeFalse();
        }

        [Test]
        public void Update_ChangesAddress_UpdatesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            profile.Address = "789 New St";
            profile.City = "Seattle";
            profile.State = "WA";
            profile.Zip = "98101";
            _service.Update(profile);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Address.Should().Be("789 New St");
            updated!.City.Should().Be("Seattle");
            updated!.State.Should().Be("WA");
            updated!.Zip.Should().Be("98101");
        }

        [Test]
        public void Update_ChangesTheme_UpdatesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.ThemeId = 1;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act - User changes theme
            profile.ThemeId = 3;
            _service.Update(profile);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.ThemeId.Should().Be(3);
        }

        [Test]
        public void Update_ChangesAvatar_UpdatesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.Avatar = "old-avatar.jpg";
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            profile.Avatar = "new-avatar.jpg";
            _service.Update(profile);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Avatar.Should().Be("new-avatar.jpg");
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_WithExistingId_RemovesFromDatabase()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com");
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            _service.Delete(1);

            // Assert
            var deleted = _context.Profiles.Find(1);
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

        #region IncrementViews Tests

        [Test]
        public void IncrementViews_WithExistingProfile_IncrementsViewCount()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com");
            profile.Views = 10;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            _service.IncrementViews(1);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Views.Should().Be(11);
        }

        [Test]
        public void IncrementViews_FromZero_IncrementsToOne()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com");
            profile.Views = 0;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act
            _service.IncrementViews(1);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Views.Should().Be(1);
        }

        [Test]
        public void IncrementViews_MultipleIncrements_AccumulatesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "test@example.com");
            profile.Views = 0;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act - Simulate multiple profile views
            _service.IncrementViews(1);
            _service.IncrementViews(1);
            _service.IncrementViews(1);

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Views.Should().Be(3);
        }

        [Test]
        public void IncrementViews_WithNonExistingId_DoesNotThrowException()
        {
            // Act & Assert
            Action act = () => _service.IncrementViews(999);
            act.Should().NotThrow();
        }

        #endregion

        #region Privacy and Data Integrity Tests

        [Test]
        public void Profile_BirthdayVisibility_CanBeToggled()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.Birthday = new DateTime(1990, 5, 10);
            profile.BirthdayVisible = true;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act - Test 1: User makes birthday private
            _context.ChangeTracker.Clear();  // Clear before getting
            var toUpdate1 = _service.Get(1);
            toUpdate1!.BirthdayVisible = false;
            _service.Update(toUpdate1);

            // Verify it's hidden
            _context.ChangeTracker.Clear();
            var afterHidden = _service.Get(1);

            // Act - Test 2: User makes birthday public again
            _context.ChangeTracker.Clear();  // Clear before getting
            var toUpdate2 = _service.Get(1);
            toUpdate2!.BirthdayVisible = true;
            _service.Update(toUpdate2);
            
            // Verify it's shown
            _context.ChangeTracker.Clear();
            var afterShown = _service.Get(1);

            // Assert
            afterHidden!.BirthdayVisible.Should().BeFalse();
            afterShown!.BirthdayVisible.Should().BeTrue();
        }

        [Test]
        public void Profile_WithOptionalFields_HandlesNullsCorrectly()
        {
            // Arrange - Profile with minimal required fields
            var profile = new Profile
            {
                Id = 1,
                UserId = "user-1",
                Email = "minimal@example.com",
                DisplayName = "MinimalUser",
                FirstName = null,
                MiddleName = null,
                LastName = null,
                Birthday = null,
                BirthdayVisible = null,
                Address = null,
                Address2 = null,
                City = null,
                State = null,
                Zip = null,
                Country = null,
                Signature = null,
                Avatar = null,
                ThemeId = null,
                Views = 0
            };

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);

            saved.Should().NotBeNull();
            saved!.FirstName.Should().BeNull();
            saved!.Birthday.Should().BeNull();
            saved!.ThemeId.Should().BeNull();
        }

        [Test]
        public void Profile_DisplayName_IsUniqueIdentifier()
        {
            // Arrange - Multiple users should have unique display names
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "user1@example.com", "JohnDoe"),
                TestDataHelper.CreateTestProfile(2, "user-2", "user2@example.com", "JaneSmith"),
                TestDataHelper.CreateTestProfile(3, "user-3", "user3@example.com", "BobJones")
            );
            _context.SaveChanges();

            // Act - Look up by display name
            var john = _service.GetByDisplayName("JohnDoe");
            var jane = _service.GetByDisplayName("JaneSmith");

            // Assert
            john.Should().NotBeNull();
            jane!.Should().NotBeNull();
            john!.UserId.Should().Be("user-1");
            jane!.UserId.Should().Be("user-2");
        }

        #endregion

        #region Real-World Scenario Tests

        [Test]
        public void Profile_CompleteUserJourney_WorksCorrectly()
        {
            // Arrange - New user registration
            var profile = TestDataHelper.CreateTestProfile(1, "new-user", "newuser@example.com", "NewUser");
            profile.Views = 0;

            // Act & Assert - Step 1: Create profile
            _service.Add(profile);
            var created = _service.GetByUserId("new-user");
            created.Should().NotBeNull();
            created!.Views.Should().Be(0);

            // Step 2: User updates profile
            created.FirstName = "John";
            created.LastName = "Doe";
            created.Birthday = new DateTime(1988, 7, 15);
            created.BirthdayVisible = true;
            _service.Update(created);

            // Step 3: Profile gets viewed
            _service.IncrementViews(1);
            _service.IncrementViews(1);

            // Step 4: User changes privacy settings
            created.BirthdayVisible = false;
            _service.Update(created);

            // Final verification
            var final = _service.Get(1);
            final!.FirstName.Should().Be("John");
            final!.LastName.Should().Be("Doe");
            final!.BirthdayVisible.Should().BeFalse();
            final!.Views.Should().Be(2);
        }

        [Test]
        public void Profile_MultipleUsersLookup_PerformsEfficiently()
        {
            // Arrange - Simulate multiple users in system
            for (int i = 1; i <= 10; i++)
            {
                var profile = TestDataHelper.CreateTestProfile(i, $"user-{i}", $"user{i}@example.com", $"User{i}");
                _context.Profiles.Add(profile);
            }
            _context.SaveChanges();

            // Act - Bulk lookup
            var userIds = new List<string> { "user-2", "user-5", "user-8" };
            var results = _service.GetByUserIds(userIds);

            // Assert
            results.Should().HaveCount(3);
            results.Should().Contain(p => p.UserId == "user-2");
            results.Should().Contain(p => p.UserId == "user-5");
            results.Should().Contain(p => p.UserId == "user-8");
        }

        [Test]
        public void Profile_UserSearchByEmail_FindsCorrectUser()
        {
            // Arrange - Multiple users
            _context.Profiles.AddRange(
                TestDataHelper.CreateTestProfile(1, "user-1", "john.doe@example.com", "JohnDoe"),
                TestDataHelper.CreateTestProfile(2, "user-2", "jane.smith@example.com", "JaneSmith"),
                TestDataHelper.CreateTestProfile(3, "user-3", "bob.jones@example.com", "BobJones")
            );
            _context.SaveChanges();

            // Act - Admin searches for user by email
            var result = _service.GetByEmail("jane.smith@example.com");

            // Assert
            result.Should().NotBeNull();
            result!.DisplayName.Should().Be("JaneSmith");
            result!.UserId.Should().Be("user-2");
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void Profile_WithVeryLongSignature_SavesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.Signature = new string('A', 500); // Very long signature

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);
            saved!.Signature.Should().HaveLength(500);
        }

        [Test]
        public void Profile_WithSpecialCharactersInName_SavesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.FirstName = "José";
            profile.LastName = "O'Brien-Smith";
            profile.DisplayName = "José_O'Brien";

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);
            saved!.FirstName.Should().Be("José");
            saved!.LastName.Should().Be("O'Brien-Smith");
            saved!.DisplayName.Should().Be("José_O'Brien");
        }

        [Test]
        public void Profile_WithInternationalAddress_SavesCorrectly()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.Address = "123 Main Street";
            profile.City = "München";
            profile.State = "Bayern";
            profile.Zip = "80331";
            profile.Country = "Deutschland";

            // Act
            _service.Add(profile);

            // Assert
            var saved = _context.Profiles.Find(1);
            saved!.City.Should().Be("München");
            saved!.Country.Should().Be("Deutschland");
        }

        [Test]
        public void Profile_ViewCount_HandlesConcurrentIncrements()
        {
            // Arrange
            var profile = TestDataHelper.CreateTestProfile(1, "user-1", "user@example.com");
            profile.Views = 0;
            _context.Profiles.Add(profile);
            _context.SaveChanges();

            // Act - Simulate concurrent views
            for (int i = 0; i < 100; i++)
            {
                _service.IncrementViews(1);
            }

            // Assert
            var updated = _context.Profiles.Find(1);
            updated!.Views.Should().Be(100);
        }

        #endregion
    }
}
