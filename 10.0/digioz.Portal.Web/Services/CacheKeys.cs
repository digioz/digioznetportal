using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Central place for shared IMemoryCache keys so admin pages can invalidate them.
    /// </summary>
    public static class CacheKeys
    {
        public const string ConfigDictionary = "ConfigDictionary";
        public const string CommentsHelperConfigs = "CommentsHelper_Configs";
        public const string CommentsHelperCommentConfigs = "CommentsHelper_CommentConfigs";
        public const string DefaultTheme = "Theme_Default";
        private const string ThemePrefix = "Theme_";

        public static string Theme(int id) => ThemePrefix + id;

        /// <summary>
        /// Remove cache entries derived from the Theme table.
        /// </summary>
        public static void InvalidateTheme(IMemoryCache cache, int? id = null)
        {
            cache.Remove(DefaultTheme);
            if (id.HasValue)
            {
                cache.Remove(Theme(id.Value));
            }
        }

        /// <summary>
        /// Remove all cache entries derived from the Config table.
        /// </summary>
        public static void InvalidateConfig(IMemoryCache cache)
        {
            cache.Remove(ConfigDictionary);
            cache.Remove(CommentsHelperConfigs);
        }

        /// <summary>
        /// Remove all cache entries derived from the CommentConfig table.
        /// </summary>
        public static void InvalidateCommentConfig(IMemoryCache cache)
        {
            cache.Remove(CommentsHelperCommentConfigs);
        }
    }
}
