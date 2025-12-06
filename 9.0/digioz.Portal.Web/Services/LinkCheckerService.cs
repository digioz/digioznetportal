using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Dal.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Service for checking link validity and updating link metadata
    /// </summary>
    public class LinkCheckerService : IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LinkCheckerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);
        private bool _disposed = false;

        // Constants for optimized description extraction
        private const int MaxHtmlBytesToRead = 51200; // 50KB - sufficient for most <head> sections
        private static readonly byte[] HeadCloseTagBytes = Encoding.UTF8.GetBytes("</head>");

        public LinkCheckerService(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<LinkCheckerService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Checks all links in batches to avoid timeouts
        /// </summary>
        public async Task<List<LinkCheckResult>> CheckAllLinksAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            var results = new List<LinkCheckResult>();
            List<Link> allLinks;

            // Get all links using a dedicated scope
            using (var scope = _scopeFactory.CreateScope())
            {
                var linkService = scope.ServiceProvider.GetRequiredService<ILinkService>();
                allLinks = linkService.GetAll();
            }
            
            _logger.LogInformation($"Starting link check for {allLinks.Count} links");

            // Process in batches sequentially to avoid DbContext threading issues
            for (int i = 0; i < allLinks.Count; i += batchSize)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var batch = allLinks.Skip(i).Take(batchSize).ToList();
                var batchResults = await CheckLinkBatchAsync(batch, cancellationToken);
                results.AddRange(batchResults);

                _logger.LogInformation($"Processed batch {(i / batchSize) + 1}/{(int)Math.Ceiling((double)allLinks.Count / batchSize)}");
            }

            _logger.LogInformation($"Link check completed. Processed {results.Count} links");
            return results;
        }

        /// <summary>
        /// Checks a batch of links - performs HTTP checks concurrently but database updates sequentially
        /// </summary>
        private async Task<List<LinkCheckResult>> CheckLinkBatchAsync(List<Link> links, CancellationToken cancellationToken)
        {
            var results = new List<LinkCheckResult>();
            
            // Check all links in the batch concurrently (HTTP operations are thread-safe)
            // Each task works with its own LinkUpdateInfo to avoid race conditions
            var checkTasks = links.Select(async link =>
            {
                var result = await CheckLinkStatusAsync(link, cancellationToken);
                
                // Create update info with the changes to be applied
                LinkUpdateInfo updateInfo = null;
                if (result.WasUpdated)
                {
                    updateInfo = new LinkUpdateInfo
                    {
                        LinkId = link.Id,
                        Visible = link.Visible,
                        Description = link.Description
                    };
                }
                
                return (result, updateInfo);
            });
            
            var checkResults = await Task.WhenAll(checkTasks);

            // Update database sequentially with a fresh scope for each update
            // This ensures each update has its own DbContext instance
            foreach (var (result, updateInfo) in checkResults)
            {
                if (updateInfo != null)
                {
                    await _dbSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        // Create a new scope for each database update to ensure DbContext isolation
                        using var scope = _scopeFactory.CreateScope();
                        var linkService = scope.ServiceProvider.GetRequiredService<ILinkService>();
                        
                        // Get fresh entity from database to avoid detached entity issues
                        var freshLink = linkService.Get(updateInfo.LinkId);
                        if (freshLink != null)
                        {
                            // Apply the changes to the fresh entity
                            freshLink.Visible = updateInfo.Visible;
                            freshLink.Description = updateInfo.Description;
                            linkService.Update(freshLink);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to update link {updateInfo.LinkId} in database");
                        result.Message += " (DB update failed)";
                    }
                    finally
                    {
                        _dbSemaphore.Release();
                    }
                }
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Checks a single link status (HTTP check only, no database update)
        /// </summary>
        private async Task<LinkCheckResult> CheckLinkStatusAsync(Link link, CancellationToken cancellationToken)
        {
            var result = new LinkCheckResult
            {
                LinkId = link.Id,
                LinkName = link.Name,
                Url = link.Url,
                CheckedAt = DateTime.UtcNow,
                WasUpdated = false
            };

            try
            {
                if (string.IsNullOrWhiteSpace(link.Url))
                {
                    result.Status = LinkCheckStatus.DeadLink;
                    result.Message = "URL is empty";
                    return result;
                }

                // Validate URL to prevent SSRF attacks
                if (!IsValidAndSafeUrl(link.Url, out string validationError))
                {
                    result.Status = LinkCheckStatus.DeadLink;
                    result.Message = $"Invalid URL: {validationError}";
                    _logger.LogWarning($"Rejected unsafe URL during link check: {link.Url} - {validationError}");
                    return result;
                }

                // Create HttpClient from factory (properly managed lifecycle)
                using var httpClient = _httpClientFactory.CreateClient("LinkChecker");

                HttpResponseMessage response = null;

                // Try HEAD request first (faster), then fall back to GET if needed
                try
                {
                    using var headRequest = new HttpRequestMessage(HttpMethod.Head, link.Url);
                    response = await httpClient.SendAsync(headRequest, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // Some servers don't support HEAD, try GET
                    response?.Dispose();
                    response = null;
                    
                    using var getRequest = new HttpRequestMessage(HttpMethod.Get, link.Url);
                    response = await httpClient.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                }
                catch
                {
                    // Ensure response is disposed on any other exception (timeout, cancellation, etc.)
                    response?.Dispose();
                    throw;
                }

                // Use using statement to ensure response is disposed
                using (response)
                {
                    result.HttpStatusCode = (int)response.StatusCode;

                    // Check response status and update link object (but don't save to DB yet)
                    if (response.IsSuccessStatusCode)
                    {
                        // 2xx - Success
                        result.Status = LinkCheckStatus.Success;
                        result.Message = $"Link is valid ({response.StatusCode})";

                        // Check if we need to update description
                        if (string.IsNullOrWhiteSpace(link.Description))
                        {
                            var description = await ExtractDescriptionAsync(link.Url, httpClient, cancellationToken);
                            if (!string.IsNullOrWhiteSpace(description))
                            {
                                link.Description = $"[DESCRIPTION UPDATED] {description}";
                                result.Status = LinkCheckStatus.DescriptionUpdated;
                                result.Message = "Description extracted and updated";
                                result.WasUpdated = true;
                            }
                        }
                    }
                    else if (IsRedirectStatus(response.StatusCode))
                    {
                        // 301, 302, 307, 308 - Redirects
                        link.Visible = false;
                        link.Description = PrependTag("[REDIRECT LINK]", link.Description);
                        result.Status = LinkCheckStatus.RedirectLink;
                        result.Message = $"Redirect detected ({response.StatusCode})";
                        result.WasUpdated = true;
                    }
                    else if (IsServerError(response.StatusCode))
                    {
                        // 500, 503 - Server errors
                        link.Visible = false;
                        link.Description = PrependTag("[ERROR LINK]", link.Description);
                        result.Status = LinkCheckStatus.ErrorLink;
                        result.Message = $"Server error ({response.StatusCode})";
                        result.WasUpdated = true;
                    }
                    else if (IsClientError(response.StatusCode))
                    {
                        // 400, 403, 404 - Client errors
                        link.Visible = false;
                        link.Description = PrependTag("[DEAD LINK]", link.Description);
                        result.Status = LinkCheckStatus.DeadLink;
                        result.Message = $"Dead link ({response.StatusCode})";
                        result.WasUpdated = true;
                    }
                    else
                    {
                        result.Status = LinkCheckStatus.Success;
                        result.Message = $"Unknown status ({response.StatusCode})";
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // User-initiated cancellation - propagate it
                _logger.LogInformation($"Link check cancelled by user for: {link.Url}");
                throw;
            }
            catch (OperationCanceledException)
            {
                // Timeout from HttpClient (15 second timeout configured)
                // This catches both OperationCanceledException and TaskCanceledException
                result.Status = LinkCheckStatus.Timeout;
                result.Message = "Request timed out (15s timeout exceeded)";
                _logger.LogWarning($"Timeout checking link: {link.Url}");
            }
            catch (HttpRequestException ex)
            {
                // Network error
                link.Visible = false;
                link.Description = PrependTag("[DEAD LINK]", link.Description);
                result.Status = LinkCheckStatus.NetworkError;
                result.Message = $"Network error: {ex.Message}";
                result.WasUpdated = true;
                _logger.LogWarning($"Network error checking link {link.Url}: {ex.Message}");
            }
            catch (Exception ex)
            {
                result.Status = LinkCheckStatus.NetworkError;
                result.Message = $"Error: {ex.Message}";
                _logger.LogError(ex, $"Error checking link: {link.Url}");
            }

            return result;
        }

        /// <summary>
        /// Validates URL to prevent SSRF attacks and ensure only safe HTTP/HTTPS URLs are checked
        /// </summary>
        private bool IsValidAndSafeUrl(string url, out string error)
        {
            error = null;

            // Try to parse the URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                error = "Invalid URL format";
                return false;
            }

            // Only allow HTTP and HTTPS schemes
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                error = $"Unsupported URL scheme '{uri.Scheme}'. Only HTTP and HTTPS are allowed";
                return false;
            }

            // Check for localhost and loopback addresses
            if (IsLocalOrLoopbackHost(uri.Host))
            {
                error = "Localhost and loopback addresses are not allowed";
                return false;
            }

            // Check for private IP ranges (RFC 1918)
            if (IsPrivateIpAddress(uri.Host))
            {
                error = "Private IP addresses are not allowed";
                return false;
            }

            // Check for link-local addresses (169.254.x.x)
            if (IsLinkLocalAddress(uri.Host))
            {
                error = "Link-local addresses are not allowed";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the host is localhost or a loopback address
        /// </summary>
        private bool IsLocalOrLoopbackHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return false;

            // Check for common localhost names
            var localhostNames = new[] { "localhost", "localhost.localdomain" };
            if (localhostNames.Contains(host.ToLowerInvariant()))
                return true;

            // Check if it's an IP address
            if (System.Net.IPAddress.TryParse(host, out var ipAddress))
            {
                // Check for loopback (127.0.0.0/8 for IPv4, ::1 for IPv6)
                return System.Net.IPAddress.IsLoopback(ipAddress);
            }

            return false;
        }

        /// <summary>
        /// Checks if the host is a private IP address (RFC 1918)
        /// </summary>
        private bool IsPrivateIpAddress(string host)
        {
            if (!System.Net.IPAddress.TryParse(host, out var ipAddress))
                return false;

            // Only check IPv4 addresses
            if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            var bytes = ipAddress.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the host is a link-local address (169.254.0.0/16)
        /// </summary>
        private bool IsLinkLocalAddress(string host)
        {
            if (!System.Net.IPAddress.TryParse(host, out var ipAddress))
                return false;

            // Only check IPv4 addresses
            if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            var bytes = ipAddress.GetAddressBytes();

            // 169.254.0.0/16 (APIPA - Automatic Private IP Addressing)
            return bytes[0] == 169 && bytes[1] == 254;
        }

        /// <summary>
        /// Extracts description from website HTML using HtmlAgilityPack (optimized to read only head section)
        /// </summary>
        private async Task<string> ExtractDescriptionAsync(string url, HttpClient httpClient, CancellationToken cancellationToken)
        {
            try
            {
                // Use ResponseHeadersRead to avoid buffering entire response
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                // Read only the portion we need (up to 50KB or </head> tag)
                var htmlChunk = await ReadHtmlHeadSectionAsync(response, cancellationToken);
                if (string.IsNullOrEmpty(htmlChunk))
                    return null;

                // Parse HTML using HtmlAgilityPack (handles malformed HTML and attribute order)
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlChunk);

                // Try to extract meta description (handles any attribute order)
                var metaDesc = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description' or @name='Description']");
                if (metaDesc != null)
                {
                    var content = metaDesc.GetAttributeValue("content", null);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return CleanDescription(content);
                    }
                }

                // Try og:description (Open Graph protocol)
                var ogDesc = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
                if (ogDesc != null)
                {
                    var content = ogDesc.GetAttributeValue("content", null);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return CleanDescription(content);
                    }
                }

                // Try Twitter description
                var twitterDesc = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']");
                if (twitterDesc != null)
                {
                    var content = twitterDesc.GetAttributeValue("content", null);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return CleanDescription(content);
                    }
                }

                // Try title as fallback
                var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
                {
                    return CleanDescription(titleNode.InnerText);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to extract description from {url}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Reads HTML content up to the closing head tag or max bytes limit
        /// </summary>
        private async Task<string> ReadHtmlHeadSectionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memoryStream = new MemoryStream();
            
            var buffer = new byte[4096]; // 4KB buffer for reading
            var totalBytesRead = 0;
            int bytesRead;

            while (totalBytesRead < MaxHtmlBytesToRead && 
                   (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                // Check if we've reached the </head> tag
                if (totalBytesRead >= HeadCloseTagBytes.Length)
                {
                    var recentBytes = memoryStream.GetBuffer().AsSpan(
                        Math.Max(0, totalBytesRead - 512), 
                        Math.Min(512, totalBytesRead)
                    );

                    if (IndexOf(recentBytes, HeadCloseTagBytes) >= 0)
                    {
                        // Found closing head tag, stop reading
                        break;
                    }
                }
            }

            // Convert the bytes we read to string
            var encoding = GetEncodingFromResponse(response) ?? Encoding.UTF8;
            return encoding.GetString(memoryStream.GetBuffer(), 0, totalBytesRead);
        }

        /// <summary>
        /// Gets the encoding from response headers, defaults to UTF-8
        /// </summary>
        private Encoding GetEncodingFromResponse(HttpResponseMessage response)
        {
            try
            {
                var charset = response.Content.Headers.ContentType?.CharSet;
                if (!string.IsNullOrEmpty(charset))
                {
                    return Encoding.GetEncoding(charset);
                }
            }
            catch
            {
                // If encoding is invalid, fall back to UTF-8
            }

            return Encoding.UTF8;
        }

        /// <summary>
        /// Helper method to find byte sequence in a span
        /// </summary>
        private int IndexOf(ReadOnlySpan<byte> span, byte[] pattern)
        {
            return span.IndexOf(pattern);
        }

        /// <summary>
        /// Cleans and truncates description
        /// </summary>
        private string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            // Decode HTML entities (HtmlAgilityPack already handles most, but double-check)
            description = WebUtility.HtmlDecode(description);
            description = description.Trim();

            // Truncate if too long
            if (description.Length > 500)
                description = description.Substring(0, 497) + "...";

            return description;
        }

        /// <summary>
        /// Prepends a tag to description if not already present
        /// </summary>
        private string PrependTag(string tag, string description)
        {
            if (string.IsNullOrEmpty(description))
                return tag;

            // Don't add duplicate tags
            if (description.StartsWith(tag))
                return description;

            return $"{tag} {description}";
        }

        private bool IsRedirectStatus(HttpStatusCode status)
        {
            return status == HttpStatusCode.MovedPermanently ||     // 301
                   status == HttpStatusCode.Found ||                 // 302
                   status == HttpStatusCode.TemporaryRedirect ||     // 307
                   status == HttpStatusCode.PermanentRedirect;       // 308
        }

        private bool IsServerError(HttpStatusCode status)
        {
            return status == HttpStatusCode.InternalServerError ||  // 500
                   status == HttpStatusCode.ServiceUnavailable;     // 503
        }

        private bool IsClientError(HttpStatusCode status)
        {
            return status == HttpStatusCode.BadRequest ||           // 400
                   status == HttpStatusCode.Forbidden ||            // 403
                   status == HttpStatusCode.NotFound;               // 404
        }

        /// <summary>
        /// Disposes resources used by the service
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources used by the service
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // HttpClient is now managed by IHttpClientFactory, no need to dispose
                    _dbSemaphore?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
