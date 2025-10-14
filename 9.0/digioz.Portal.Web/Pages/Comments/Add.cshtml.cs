using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using digioz.Portal.Utilities;
using HtmlAgilityPack; // added for sanitization
using System.Text.RegularExpressions;

namespace digioz.Portal.Web.Pages.Comments
{
    [ValidateAntiForgeryToken]
    public class AddModel : PageModel
    {
        private readonly ICommentService _commentService;
        private readonly IConfigService _configService;
        private readonly ILogger<AddModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IUserHelper _userHelper;

        public AddModel(ICommentService commentService, IConfigService configService, ILogger<AddModel> logger, IHttpClientFactory httpClientFactory, IMemoryCache cache, IUserHelper userHelper)
        {
            _commentService = commentService;
            _configService = configService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _userHelper = userHelper;
        }

        [BindProperty] public string referenceId { get; set; }
        [BindProperty] public string referenceType { get; set; }
        [BindProperty] public string comment { get; set; }
        [BindProperty] public string recaptchaToken { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (!User.Identity?.IsAuthenticated ?? true)
                return Redirect(referer ?? "/");

            if (string.IsNullOrWhiteSpace(comment) || comment.Length > 5000)
                return Redirect(referer ?? "/");

            var sanitized = Sanitize(comment);

            var cfg = _configService.GetAll();
            var recaptchaEnabled = bool.TryParse(cfg.FirstOrDefault(c => c.ConfigKey == "RecaptchaEnabled")?.ConfigValue, out var en) && en;
            if (recaptchaEnabled)
            {
                var secret = cfg.FirstOrDefault(c => c.ConfigKey == "RecaptchaPrivateKey")?.ConfigValue;
                if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(recaptchaToken) || !await VerifyRecaptchaV3Async(secret, recaptchaToken, "comment"))
                    return Redirect(referer ?? "/");
            }

            if (string.IsNullOrWhiteSpace(referenceType))
            {
                try
                {
                    if (Uri.TryCreate(referer, UriKind.Absolute, out var uri))
                        referenceType = uri.AbsolutePath;
                }
                catch { referenceType = "/Index"; }
            }
            if (string.IsNullOrWhiteSpace(referenceType) || referenceType == "/") referenceType = "/Index";

            var userName = User.Identity?.Name;
            var newComment = new Comment
            {
                Id = Guid.NewGuid().ToString(),
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                Body = sanitized,
                UserId = _userHelper.GetUserIdByEmail(userName),
                Username = userName,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Likes = 0
            };
            _commentService.Add(newComment);

            _cache.Remove("CommentsMenu_" + referenceType);
            return Redirect(referer ?? "/");
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // Parse HTML then extract plain text only; remove all tags, scripts, attributes.
            var doc = new HtmlDocument();
            doc.LoadHtml(input);
            var text = doc.DocumentNode.InnerText ?? string.Empty;
            // Collapse excessive whitespace/newlines
            text = Regex.Replace(text, "\\s+", " ").Trim();
            return text;
        }

        private class RecaptchaResponse { public bool Success { get; set; } public float Score { get; set; } public string Action { get; set; } }

        private async Task<bool> VerifyRecaptchaV3Async(string secret, string token, string action)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={Uri.EscapeDataString(secret)}&response={Uri.EscapeDataString(token)}", null);
                if (!response.IsSuccessStatusCode) return false;
                var payload = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
                return payload?.Success == true && payload.Score >= 0.5f && (string.IsNullOrEmpty(action) || payload.Action == action);
            }
            catch { return false; }
        }
    }
}
