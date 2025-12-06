using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Pages.Link
{
    [Authorize(Roles = "Administrator")]
    public class CheckLinksModel : PageModel
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public CheckLinksModel(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
        }

        public List<LinkCheckResult> Results { get; set; }
        public bool IsChecking { get; set; }
        public int TotalLinks { get; set; }
        public int UpdatedLinks { get; set; }
        public int DeadLinks { get; set; }
        public int ErrorLinks { get; set; }
        public int RedirectLinks { get; set; }
        public int SuccessfulLinks { get; set; }

        public void OnGet()
        {
            Results = new List<LinkCheckResult>();
            IsChecking = false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IsChecking = true;
            
            try
            {
                var logger = _loggerFactory.CreateLogger<LinkCheckerService>();
                using var checker = new LinkCheckerService(_scopeFactory, _httpClientFactory, logger);
                Results = await checker.CheckAllLinksAsync(batchSize: 10, CancellationToken.None);

                // Calculate statistics
                TotalLinks = Results.Count;
                UpdatedLinks = Results.Count(r => r.WasUpdated);
                DeadLinks = Results.Count(r => r.Status == LinkCheckStatus.DeadLink || r.Status == LinkCheckStatus.NetworkError);
                ErrorLinks = Results.Count(r => r.Status == LinkCheckStatus.ErrorLink);
                RedirectLinks = Results.Count(r => r.Status == LinkCheckStatus.RedirectLink);
                SuccessfulLinks = Results.Count(r => r.Status == LinkCheckStatus.Success || r.Status == LinkCheckStatus.DescriptionUpdated);

                TempData["SuccessMessage"] = $"Link check completed! {UpdatedLinks} links were updated out of {TotalLinks} total links.";
            }
            catch (System.Exception ex)
            {
                var logger = _loggerFactory.CreateLogger<CheckLinksModel>();
                logger.LogError(ex, "Error during link check operation");
                ModelState.AddModelError(string.Empty, $"An error occurred during link checking: {ex.Message}");
            }
            finally
            {
                IsChecking = false;
            }

            return Page();
        }
    }
}
