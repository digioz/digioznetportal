using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace digioz.Portal.Web.Pages
{
    public class RateLimitedModel : PageModel
    {
        public string? Reason { get; set; }
        public string? RetryAfter { get; set; }
        public int? AutoReloadSeconds { get; private set; }
        
        public void OnGet(string? reason = null, string? retryAfter = null)
        {
            Reason = reason ?? "Too many requests";
            RetryAfter = retryAfter ?? "a few minutes";
            
            CalculateAutoReload();
        }

        private void CalculateAutoReload()
        {
            if (string.IsNullOrEmpty(RetryAfter)) return;

            var lowerRetry = RetryAfter.ToLowerInvariant();
            
            // Try to extract number with specific units
            // Matches formats like "1 minute", "10 minutes", "1 hour", "30 seconds"
            var match = Regex.Match(lowerRetry, @"^(\d+)\s*(second|minute|hour)s?");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int value))
                {
                    string unit = match.Groups[2].Value;
                    if (unit == "second")
                    {
                        AutoReloadSeconds = value;
                    }
                    else if (unit == "minute")
                    {
                        AutoReloadSeconds = value * 60;
                    }
                    else if (unit == "hour")
                    {
                         AutoReloadSeconds = value * 3600;
                    }
                }
            }
        }
    }
}
