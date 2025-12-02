using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.RssManager
{
    public class DetailsModel : PageModel
    {
        private readonly IRssService _service;
        public DetailsModel(IRssService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public Rss? Item { get; private set; }
        public List<FeedEntry> FeedItems { get; private set; } = new();

        public class FeedEntry
        {
            public string Title { get; set; } = string.Empty;
            public string? Summary { get; set; }
            public DateTimeOffset PublishDate { get; set; }
            public string? Link { get; set; }
        }

        public IActionResult OnGet()
        {
            Item = _service.Get(Id);
            if (Item == null) return NotFound();

            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
                using var reader = XmlReader.Create(Item.Url, settings);
                var doc = XDocument.Load(reader);
                var root = doc.Root;
                if (root != null)
                {
                    if (root.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase))
                    {
                        var items = doc.Descendants("item").Take(Math.Max(1, Item.MaxCount));
                        FeedItems = items.Select(x => new FeedEntry
                        {
                            Title = (string?)x.Element("title") ?? "(no title)",
                            Summary = (string?)x.Element("description"),
                            PublishDate = ParseDate((string?)x.Element("pubDate")),
                            Link = (string?)x.Element("link")
                        }).ToList();
                    }
                    else if (root.Name.LocalName.Equals("feed", StringComparison.OrdinalIgnoreCase))
                    {
                        XNamespace ns = root.Name.Namespace;
                        var entries = doc.Descendants(ns + "entry").Take(Math.Max(1, Item.MaxCount));
                        FeedItems = entries.Select(x => new FeedEntry
                        {
                            Title = (string?)x.Element(ns + "title") ?? "(no title)",
                            Summary = (string?)x.Element(ns + "summary") ?? (string?)x.Element(ns + "content"),
                            PublishDate = ParseDate((string?)x.Element(ns + "updated") ?? (string?)x.Element(ns + "published")),
                            Link = x.Elements(ns + "link").FirstOrDefault()?.Attribute("href")?.Value
                        }).ToList();
                    }
                }
            }
            catch
            {
                // ignore preview errors, show empty list
            }

            return Page();
        }

        private static DateTimeOffset ParseDate(string? s)
        {
            if (DateTimeOffset.TryParse(s, out var dt)) return dt;
            return DateTimeOffset.MinValue;
        }
    }
}
