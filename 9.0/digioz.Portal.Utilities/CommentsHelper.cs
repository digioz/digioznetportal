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
    }
}
