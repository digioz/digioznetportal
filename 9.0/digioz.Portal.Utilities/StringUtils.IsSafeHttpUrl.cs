using System;
using System.Net;
using System.Net.Sockets;

namespace digioz.Portal.Utilities
{
    public static partial class StringUtils
    {
        public static bool IsSafeHttpUrl(string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return false;
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return false;

            // Enforce HTTP/HTTPS only
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

            // Allow only default ports (80/443). Adjust if you want to allow custom ports.
            var port = uri.Port;
            if ((uri.Scheme == Uri.UriSchemeHttp && port != 80) ||
                (uri.Scheme == Uri.UriSchemeHttps && port != 443)) return false;

            var host = uri.Host;
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("127.0.0.1") ||
                host.Equals("::1")) return false;

            try
            {
                var addresses = Dns.GetHostAddresses(host);
                foreach (var address in addresses)
                {
                    if (IPAddress.IsLoopback(address)) return false;

                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var bytes = address.GetAddressBytes();
                        // 0.0.0.0/8, 10.0.0.0/8
                        if (bytes[0] == 0 || bytes[0] == 10) return false;
                        // 127.0.0.0/8
                        if (bytes[0] == 127) return false;
                        // 169.254.0.0/16 (link-local)
                        if (bytes[0] == 169 && bytes[1] == 254) return false;
                        // 172.16.0.0/12
                        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
                        // 192.168.0.0/16
                        if (bytes[0] == 192 && bytes[1] == 168) return false;
                    }
                    else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        // IPv6 loopback already covered by IsLoopback
                        // Link-local/site-local or unique local (fc00::/7) should be blocked
                        if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal) return false;
                        var bytes = address.GetAddressBytes();
                        // fc00::/7 unique local
                        if ((bytes[0] & 0xFE) == 0xFC) return false;
                    }
                }
            }
            catch
            {
                // DNS resolution failures considered unsafe
                return false;
            }

            // Optionally enforce HTTPS only
            // if (uri.Scheme != Uri.UriSchemeHttps) return false;

            return true;
        }
    }
}
