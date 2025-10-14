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
    /// Helper kept DAL-agnostic by accepting a delegate that returns the current user set.
    /// </summary>
    public sealed class UserHelper : IUserHelper
    {
        private readonly Func<IEnumerable<AspNetUser>> _getUsers;

        public UserHelper(Func<IEnumerable<AspNetUser>> getUsers)
        {
            _getUsers = getUsers ?? throw new ArgumentNullException(nameof(getUsers));
        }

        public string? GetUserIdByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var lookup = email.Trim().ToUpperInvariant();
            var users = _getUsers() ?? Enumerable.Empty<AspNetUser>();
            var match = users.FirstOrDefault(u => (u.NormalizedEmail ?? u.Email?.ToUpperInvariant()) == lookup);
            return match?.Id;
        }
    }
}
