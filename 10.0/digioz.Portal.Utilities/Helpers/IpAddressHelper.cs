using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;

namespace digioz.Portal.Utilities.Helpers
{
	/// <summary>
	/// Helper class for extracting the real client IP address, 
	/// with support for Cloudflare and other proxy/load balancer scenarios.
	/// </summary>
	public static class IpAddressHelper
	{
		/// <summary>
		/// Gets the user's real IP address, checking Cloudflare headers first,
		/// then standard proxy headers, and finally falling back to the connection IP.
		/// </summary>
		/// <param name="context">The HTTP context</param>
		/// <returns>The client's IP address as a string</returns>
		public static string GetUserIPAddress(HttpContext context)
		{
			string ip = string.Empty;

			try
			{
				// Priority 1: Check Cloudflare-specific header (most reliable when behind Cloudflare)
				// CF-Connecting-IP contains the original visitor IP address
				if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfConnectingIp))
				{
					var cfIp = cfConnectingIp.FirstOrDefault();
					if (!string.IsNullOrWhiteSpace(cfIp) && IsValidIpAddress(cfIp))
					{
						ip = cfIp;
						return NormalizeIpAddress(ip);
					}
				}

				// Priority 2: Check X-Forwarded-For header (standard proxy header)
				// This header can contain multiple IPs (client, proxy1, proxy2, ...)
				// The first IP is usually the original client IP
				if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
				{
					var forwardedIp = forwardedFor.FirstOrDefault();
					if (!string.IsNullOrWhiteSpace(forwardedIp))
					{
						// X-Forwarded-For can contain multiple IPs separated by commas
						// Format: "client, proxy1, proxy2"
						var ips = forwardedIp.Split(',', StringSplitOptions.RemoveEmptyEntries);
						if (ips.Length > 0)
						{
							var firstIp = ips[0].Trim();
							if (IsValidIpAddress(firstIp))
							{
								ip = firstIp;
								return NormalizeIpAddress(ip);
							}
						}
					}
				}

				// Priority 3: Check X-Real-IP header (another common proxy header)
				if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
				{
					var realIpValue = realIp.FirstOrDefault();
					if (!string.IsNullOrWhiteSpace(realIpValue) && IsValidIpAddress(realIpValue))
					{
						ip = realIpValue;
						return NormalizeIpAddress(ip);
					}
				}

				// Priority 4: Fallback to direct connection IP
				if (context.Connection.RemoteIpAddress != null)
				{
					ip = context.Connection.RemoteIpAddress.ToString();
					return NormalizeIpAddress(ip);
				}

				// If all else fails
				ip = "Unable to resolve";
			}
			catch (Exception)
			{
				// Silently fail and return fallback
				ip = "Unable to resolve";
			}

			return ip;
		}

		/// <summary>
		/// Validates if a string is a valid IP address (IPv4 or IPv6)
		/// </summary>
		/// <param name="ipString">The IP address string to validate</param>
		/// <returns>True if valid IP address, false otherwise</returns>
		private static bool IsValidIpAddress(string ipString)
		{
			if (string.IsNullOrWhiteSpace(ipString))
			{
				return false;
			}

			// Try to parse as IP address
			return IPAddress.TryParse(ipString, out _);
		}

		/// <summary>
		/// Normalizes IPv6 localhost to IPv4 localhost for consistency
		/// </summary>
		/// <param name="ip">The IP address to normalize</param>
		/// <returns>Normalized IP address</returns>
		private static string NormalizeIpAddress(string ip)
		{
			// Convert IPv6 localhost to IPv4 for consistency
			if (ip == "::1")
			{
				return "127.0.0.1";
			}

			return ip;
		}

		/// <summary>
		/// Gets detailed information about the client IP including all headers
		/// (useful for debugging/logging)
		/// </summary>
		/// <param name="context">The HTTP context</param>
		/// <returns>A formatted string with IP information</returns>
		public static string GetIpAddressDebugInfo(HttpContext context)
		{
			var info = new System.Text.StringBuilder();
			info.AppendLine("=== IP Address Debug Info ===");
			
			// Cloudflare headers
			if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp))
			{
				info.AppendLine($"CF-Connecting-IP: {cfIp}");
			}
			
			if (context.Request.Headers.TryGetValue("CF-Ray", out var cfRay))
			{
				info.AppendLine($"CF-Ray: {cfRay}");
			}
			
			// Standard proxy headers
			if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff))
			{
				info.AppendLine($"X-Forwarded-For: {xff}");
			}
			
			if (context.Request.Headers.TryGetValue("X-Real-IP", out var xRealIp))
			{
				info.AppendLine($"X-Real-IP: {xRealIp}");
			}
			
			// Connection IP
			if (context.Connection.RemoteIpAddress != null)
			{
				info.AppendLine($"Connection.RemoteIpAddress: {context.Connection.RemoteIpAddress}");
			}
			
			// Final resolved IP
			info.AppendLine($"Resolved IP: {GetUserIPAddress(context)}");
			
			return info.ToString();
		}
	}
}
