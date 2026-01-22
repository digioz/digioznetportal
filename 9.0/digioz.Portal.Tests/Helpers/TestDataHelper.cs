using Microsoft.EntityFrameworkCore;
using digioz.Portal.Dal;
using digioz.Portal.Bo;
using System;
using System.Collections.Generic;

namespace digioz.Portal.Tests.Helpers
{
    /// <summary>
    /// Helper class for creating test data and managing test database contexts
    /// </summary>
    public static class TestDataHelper
    {
        /// <summary>
        /// Creates an in-memory database context for testing
        /// </summary>
        public static digiozPortalContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<digiozPortalContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new digiozPortalContext(options);
        }

        /// <summary>
        /// Creates a sample page for testing
        /// </summary>
        public static Page CreateTestPage(int id = 1, bool visible = true)
        {
            return new Page
            {
                Id = id,
                Title = $"Test Page {id}",
                Url = $"test-page-{id}",
                Body = $"Test content for page {id}",
                Visible = visible,
                UserId = "test-user-id",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a sample comment for testing
        /// </summary>
        public static Comment CreateTestComment(string id = "1", string userId = "test-user")
        {
            return new Comment
            {
                Id = id,
                Body = "Test comment",
                UserId = userId,
                Username = "TestUser",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a sample poll for testing
        /// </summary>
        public static Poll CreateTestPoll(string id = "1", bool visible = true, bool approved = true)
        {
            return new Poll
            {
                Id = id,
                Slug = "Test poll question?",
                UserId = "test-user",
                Visible = visible,
                Approved = approved,
                DateCreated = DateTime.UtcNow,
                AllowMultipleOptionsVote = false
            };
        }

        /// <summary>
        /// Creates sample poll answers for testing
        /// </summary>
        public static List<PollAnswer> CreateTestPollAnswers(string pollId, int count = 3)
        {
            var answers = new List<PollAnswer>();
            for (int i = 1; i <= count; i++)
            {
                answers.Add(new PollAnswer
                {
                    Id = $"{pollId}-answer-{i}",
                    PollId = pollId,
                    Answer = $"Answer {i}"
                });
            }
            return answers;
        }

        /// <summary>
        /// Creates a sample link for testing
        /// </summary>
        public static Link CreateTestLink(int id = 1, bool visible = true, bool approved = false)
        {
            return new Link
            {
                Id = id,
                Name = $"Test Link {id}",
                Url = $"https://example.com/test-{id}",
                Description = $"Test link description {id}",
                Visible = visible,
                LinkCategory = 1,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a sample order for testing
        /// </summary>
        public static Order CreateTestOrder(string id = "1", string userId = "test-user")
        {
            return new Order
            {
                Id = id,
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Phone = "555-1234",
                BillingAddress = "123 Test St",
                BillingCity = "Test City",
                BillingState = "TS",
                BillingZip = "12345",
                BillingCountry = "Test Country",
                ShippingAddress = "123 Test St",
                ShippingCity = "Test City",
                ShippingState = "TS",
                ShippingZip = "12345",
                ShippingCountry = "Test Country",
                Total = 99.99m,
                TrxApproved = true
            };
        }

        /// <summary>
        /// Seeds a database context with sample data for testing
        /// </summary>
        public static void SeedTestData(digiozPortalContext context)
        {
            // Add pages
            context.Pages.AddRange(
                CreateTestPage(1, true),
                CreateTestPage(2, true),
                CreateTestPage(3, false)
            );

            // Add links
            context.Links.AddRange(
                CreateTestLink(1, true, true),
                CreateTestLink(2, true, false),
                CreateTestLink(3, false, true)
            );

            // Add polls
            var poll = CreateTestPoll("poll-1", true, true);
            context.Polls.Add(poll);
            context.PollAnswers.AddRange(CreateTestPollAnswers("poll-1"));

            context.SaveChanges();
        }

        /// <summary>
        /// Clears all data from a test database context
        /// </summary>
        public static void ClearTestData(digiozPortalContext context)
        {
            context.Pages.RemoveRange(context.Pages);
            context.Links.RemoveRange(context.Links);
            context.Polls.RemoveRange(context.Polls);
            context.PollAnswers.RemoveRange(context.PollAnswers);
            context.Comments.RemoveRange(context.Comments);
            context.Orders.RemoveRange(context.Orders);
            context.SaveChanges();
        }
    }
}
