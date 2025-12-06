using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Service for checking link validity and updating link metadata
    /// </summary>
    public class LinkCheckerService
    {
        private readonly ILinkService _linkService;
        private readonly ILogger<LinkCheckerService> _logger;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

        public LinkCheckerService(ILinkService linkService, ILogger<LinkCheckerService> logger)
        {
            _linkService = linkService;
            _logger = logger;
            
            // Configure HttpClient with reasonable timeouts
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false, // Don't follow redirects automatically
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            
            // Set a common user agent to avoid being blocked
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        /// <summary>
        /// Checks all links in batches to avoid timeouts
        /// </summary>
        public async Task<List<LinkCheckResult>> CheckAllLinksAsync(int batchSize = 10, CancellationToken cancellationToken = default)
        {
            var results = new List<LinkCheckResult>();
            var allLinks = _linkService.GetAll();
            
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
            var checkTasks = links.Select(async link =>
            {
                var result = await CheckLinkStatusAsync(link, cancellationToken);
                return (link, result);
            });
            
            var checkResults = await Task.WhenAll(checkTasks);

            // Update database sequentially (DbContext is not thread-safe)
            foreach (var (link, result) in checkResults)
            {
                if (result.WasUpdated)
                {
                    await _dbSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        _linkService.Update(link);
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

                // Make HEAD request first (faster)
                var request = new HttpRequestMessage(HttpMethod.Head, link.Url);
                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.SendAsync(request, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // Some servers don't support HEAD, try GET
                    request = new HttpRequestMessage(HttpMethod.Get, link.Url);
                    response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                }

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
                        var description = await ExtractDescriptionAsync(link.Url, cancellationToken);
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
            catch (TaskCanceledException)
            {
                result.Status = LinkCheckStatus.Timeout;
                result.Message = "Request timed out";
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
        /// Extracts description from website HTML
        /// </summary>
        private async Task<string> ExtractDescriptionAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                var html = await response.Content.ReadAsStringAsync(cancellationToken);

                // Try to extract meta description
                var metaDescMatch = Regex.Match(html, @"<meta\s+name=[""']description[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                if (metaDescMatch.Success)
                {
                    return CleanDescription(metaDescMatch.Groups[1].Value);
                }

                // Try og:description
                var ogDescMatch = Regex.Match(html, @"<meta\s+property=[""']og:description[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                if (ogDescMatch.Success)
                {
                    return CleanDescription(ogDescMatch.Groups[1].Value);
                }

                // Try title as fallback
                var titleMatch = Regex.Match(html, @"<title>([^<]+)</title>", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                {
                    return CleanDescription(titleMatch.Groups[1].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to extract description from {url}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Cleans and truncates description
        /// </summary>
        private string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            // Decode HTML entities and clean up
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
    }
}
