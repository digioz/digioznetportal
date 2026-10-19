using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;

namespace digioz.Portal.Utilities
{
    public interface ICommentsHelper
    {
        bool IsCommentsEnabledForPageTitle(string pageTitle);
        bool IsCommentsEnabledForAnnouncement(int announcementId);
        Dictionary<int, bool> AreCommentsEnabledForAnnouncements(IEnumerable<int> announcementIds);
        bool IsCommentsEnabledForPicture(int pictureId);
        bool IsCommentsEnabledForVideo(int videoId);
    }

    // This helper avoids referencing DAL by accepting data providers via delegates
    public class CommentsHelper : ICommentsHelper
    {
        private readonly Func<IEnumerable<Config>> _getConfigs;
        private readonly Func<IEnumerable<CommentConfig>> _getCommentConfigs;

        public CommentsHelper(
            Func<IEnumerable<Config>> getConfigs,
            Func<IEnumerable<CommentConfig>> getCommentConfigs)
        {
            _getConfigs = getConfigs ?? throw new ArgumentNullException(nameof(getConfigs));
            _getCommentConfigs = getCommentConfigs ?? throw new ArgumentNullException(nameof(getCommentConfigs));
        }

        public bool IsCommentsEnabledForPageTitle(string pageTitle)
        {
            if (string.IsNullOrWhiteSpace(pageTitle))
                return false;

            var configs = _getConfigs() ?? Enumerable.Empty<Config>();

            var enableAllConfig = configs.FirstOrDefault(c => c.ConfigKey == "EnableCommentsOnAllPages");
            if (enableAllConfig != null && bool.TryParse(enableAllConfig.ConfigValue, out var enableAll) && enableAll)
                return true;

            var commentConfigs = _getCommentConfigs() ?? Enumerable.Empty<CommentConfig>();
            var anyForPage = commentConfigs.Any(cc => cc.ReferenceTitle == pageTitle && cc.Visible);
            return anyForPage;
        }

        public bool IsCommentsEnabledForAnnouncement(int announcementId)
        {
            var configs = _getConfigs() ?? Enumerable.Empty<Config>();

            var enableAllConfig = configs.FirstOrDefault(c => c.ConfigKey == "EnableCommentsOnAllPages");
            if (enableAllConfig != null && bool.TryParse(enableAllConfig.ConfigValue, out var enableAll) && enableAll)
                return true;

            var commentConfigs = _getCommentConfigs() ?? Enumerable.Empty<CommentConfig>();
            var anyForAnnouncement = commentConfigs.Any(cc => 
                cc.ReferenceType == "/Announcements" && 
                cc.ReferenceId == announcementId.ToString() && 
                cc.Visible);
            return anyForAnnouncement;
        }

        public bool IsCommentsEnabledForPicture(int pictureId)
        {
            var configs = _getConfigs() ?? Enumerable.Empty<Config>();

            var enableAllConfig = configs.FirstOrDefault(c => c.ConfigKey == "EnableCommentsOnAllPages");
            if (enableAllConfig != null && bool.TryParse(enableAllConfig.ConfigValue, out var enableAll) && enableAll)
                return true;

            var commentConfigs = _getCommentConfigs() ?? Enumerable.Empty<CommentConfig>();
            var anyForPicture = commentConfigs.Any(cc => 
                cc.ReferenceType == "/Pictures/Details" && 
                cc.ReferenceId == pictureId.ToString() && 
                cc.Visible);
            return anyForPicture;
        }

        public bool IsCommentsEnabledForVideo(int videoId)
        {
            var configs = _getConfigs() ?? Enumerable.Empty<Config>();

            var enableAllConfig = configs.FirstOrDefault(c => c.ConfigKey == "EnableCommentsOnAllPages");
            if (enableAllConfig != null && bool.TryParse(enableAllConfig.ConfigValue, out var enableAll) && enableAll)
                return true;

            var commentConfigs = _getCommentConfigs() ?? Enumerable.Empty<CommentConfig>();
            var anyForVideo = commentConfigs.Any(cc => 
                cc.ReferenceType == "/Videos/Details" && 
                cc.ReferenceId == videoId.ToString() && 
                cc.Visible);
            return anyForVideo;
        }

        /// <summary>
        /// Batch method to check comments enabled status for multiple announcements at once.
        /// This avoids N+1 query issues by fetching configuration data once.
        /// </summary>
        /// <param name="announcementIds">Collection of announcement IDs to check</param>
        /// <returns>Dictionary mapping announcement ID to whether comments are enabled</returns>
        public Dictionary<int, bool> AreCommentsEnabledForAnnouncements(IEnumerable<int> announcementIds)
        {
            var result = new Dictionary<int, bool>();
            
            if (announcementIds == null || !announcementIds.Any())
                return result;

            // Fetch configs once
            var configs = _getConfigs() ?? Enumerable.Empty<Config>();
            var enableAllConfig = configs.FirstOrDefault(c => c.ConfigKey == "EnableCommentsOnAllPages");
            var enableAll = enableAllConfig != null && bool.TryParse(enableAllConfig.ConfigValue, out var enable) && enable;

            // If comments are enabled globally, all announcements have comments enabled
            if (enableAll)
            {
                foreach (var id in announcementIds)
                {
                    result[id] = true;
                }
                return result;
            }

            // Fetch comment configs once
            var commentConfigs = (_getCommentConfigs() ?? Enumerable.Empty<CommentConfig>())
                .Where(cc => cc.ReferenceType == "/Announcements" && cc.Visible)
                .ToList();

            // Build a HashSet of enabled announcement IDs for quick lookup
            var enabledAnnouncementIds = new HashSet<string>(
                commentConfigs.Select(cc => cc.ReferenceId)
            );

            // Check each announcement
            foreach (var id in announcementIds)
            {
                result[id] = enabledAnnouncementIds.Contains(id.ToString());
            }

            return result;
        }
    }
}
