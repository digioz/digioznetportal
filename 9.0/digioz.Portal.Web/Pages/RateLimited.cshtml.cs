using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages
{
    public class RateLimitedModel : PageModel
    {
        public string? Reason { get; set; }
        public string? RetryAfter { get; set; }
        
        public void OnGet(string? reason = null, string? retryAfter = null)
        {
            Reason = reason ?? "Too many requests";
            RetryAfter = retryAfter ?? "a few minutes";
        }
    }
}
