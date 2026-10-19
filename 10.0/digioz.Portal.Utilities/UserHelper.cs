#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;

namespace digioz.Portal.Utilities
{
    public interface IUserHelper
    {
        /// <summary>
        /// Returns the AspNetUsers.Id (GUID string) for the supplied email (case-insensitive).
        /// Returns null if not found or email is empty.
        /// </summary>
        string? GetUserIdByEmail(string email);
    }

    /// <summary>
    /// Helper kept DAL-agnostic by accepting a delegate that resolves a user id from an email.
    /// </summary>
    public sealed class UserHelper : IUserHelper
    {
        private readonly Func<string, string?> _getUserIdByEmail;

        public UserHelper(Func<string, string?> getUserIdByEmail)
        {
            _getUserIdByEmail = getUserIdByEmail ?? throw new ArgumentNullException(nameof(getUserIdByEmail));
        }

        public string? GetUserIdByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return _getUserIdByEmail(email.Trim());
        }
    }
}
