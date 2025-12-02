using System;

namespace digioz.Portal.Utilities
{
    public static partial class StringUtils
    {
        public static bool IsSafeHttpUrl(string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return false;
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return false;
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
