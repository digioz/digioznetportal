using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Encodings.Web;
using System.Xml;
using System.Xml.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.RssManager
{
    public class AddModel : PageModel
    {
        private readonly IRssService _service;
        private readonly UrlEncoder _urlEncoder;
        public AddModel(IRssService service, UrlEncoder urlEncoder)
        {
            _service = service;
            _urlEncoder = urlEncoder;
        }

        [BindProperty]
        public InputModel Item { get; set; } = new();

        public class InputModel
        {
            [Required, StringLength(128)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Feed URL")]
            public string Url { get; set; } = string.Empty;

            [Range(1, 100)]
            [Display(Name = "Max Count")]
            public int MaxCount { get; set; } = 10;
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            // Sanitize Name: remove HTML, encode special characters
            var sanitizedName = StripHtml(Item.Name).Trim();
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                ModelState.AddModelError("Item.Name", "Name is required.");
                return Page();
            }

            // Validate URL format and ensure it's RSS/Atom
            if (!Uri.TryCreate(Item.Url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("Item.Url", "A valid absolute HTTP/HTTPS URL is required.");
                return Page();
            }
            if (ContainsSuspiciousQuery(uri))
            {
                ModelState.AddModelError("Item.Url", "Suspicious content detected in URL.");
                return Page();
            }

            // Probe feed
            if (!IsRssOrAtom(uri))
            {
                ModelState.AddModelError("Item.Url", "The URL does not appear to be a valid RSS or Atom feed.");
                return Page();
            }

            var entity = new Rss
            {
                Name = WebUtility.HtmlEncode(sanitizedName),
                Url = uri.ToString(),
                MaxCount = Item.MaxCount,
                Timestamp = DateTime.UtcNow
            };

            _service.Add(entity);
            return RedirectToPage("/RssManager/Index", new { area = "Admin" });
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }

        private static bool ContainsSuspiciousQuery(Uri uri)
        {
            var q = uri.Query?.ToLowerInvariant() ?? string.Empty;
            if (q.Contains("<") || q.Contains(">") || q.Contains("script") || q.Contains("--") || q.Contains(";") || q.Contains("%3c") || q.Contains("%3e"))
                return true;
            return false;
        }

        private static bool IsRssOrAtom(Uri url)
        {
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
                using var reader = XmlReader.Create(url.ToString(), settings);
                var doc = XDocument.Load(reader);
                var root = doc.Root;
                if (root == null) return false;
                var name = root.Name.LocalName.ToLowerInvariant();
                return name == "rss" || name == "feed"; // rss2/atom
            }
            catch
            {
                return false;
            }
        }
    }
}
