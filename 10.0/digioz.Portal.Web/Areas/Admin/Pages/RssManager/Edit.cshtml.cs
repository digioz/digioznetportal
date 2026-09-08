using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.RssManager
{
    public class EditModel : PageModel
    {
        private readonly IRssService _service;
        public EditModel(IRssService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        [BindProperty] public InputModel Item { get; set; } = new();

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

        public IActionResult OnGet()
        {
            var entity = _service.Get(Id);
            if (entity == null) return NotFound();
            Item = new InputModel
            {
                Name = WebUtility.HtmlDecode(entity.Name ?? string.Empty),
                Url = entity.Url,
                MaxCount = entity.MaxCount
            };
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            var entity = _service.Get(Id);
            if (entity == null) return NotFound();

            var sanitizedName = StripHtml(Item.Name).Trim();
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                ModelState.AddModelError("Item.Name", "Name is required.");
                return Page();
            }

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
            if (!IsRssOrAtom(uri))
            {
                ModelState.AddModelError("Item.Url", "The URL does not appear to be a valid RSS or Atom feed.");
                return Page();
            }

            entity.Name = WebUtility.HtmlEncode(sanitizedName);
            entity.Url = uri.ToString();
            entity.MaxCount = Item.MaxCount;
            entity.Timestamp = DateTime.UtcNow;
            _service.Update(entity);

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
                return name == "rss" || name == "feed";
            }
            catch { return false; }
        }
    }
}
