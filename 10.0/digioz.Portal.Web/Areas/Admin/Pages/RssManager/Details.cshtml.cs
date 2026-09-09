using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Pages.RssManager
{
    public class DetailsModel : PageModel
    {
        private readonly IRssService _service;
        private readonly ILogger<DetailsModel> _logger;
        public DetailsModel(IRssService service, ILogger<DetailsModel> logger) { _service = service; _logger = logger; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public digioz.Portal.Bo.Rss? Item { get; private set; }
        public List<FeedItemViewModel> FeedItems { get; private set; } = new();

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
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 1024 * 1024 };
                using var reader = XmlReader.Create(Item.Url, settings);
                var doc = XDocument.Load(reader, LoadOptions.None);
                var root = doc.Root;
                if (root != null)
                {
                    if (root.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var x in doc.Descendants("item").Take(Math.Max(1, Item.MaxCount)))
                        {
                            var link = (string?)x.Element("link") ?? string.Empty;
                            if (!StringUtils.IsSafeHttpUrl(link)) link = string.Empty;
                            FeedItems.Add(new FeedItemViewModel
                            {
                                Title = (string?)x.Element("title") ?? "(no title)",
                                Summary = (string?)x.Element("description") ?? string.Empty,
                                Link = link
                            });
                        }
                    }
                    else if (root.Name.LocalName.Equals("feed", StringComparison.OrdinalIgnoreCase))
                    {
                        XNamespace ns = root.Name.Namespace;
                        foreach (var x in doc.Descendants(ns + "entry").Take(Math.Max(1, Item.MaxCount)))
                        {
                            var link = x.Elements(ns + "link").FirstOrDefault()?.Attribute("href")?.Value ?? string.Empty;
                            if (!StringUtils.IsSafeHttpUrl(link)) link = string.Empty;
                            FeedItems.Add(new FeedItemViewModel
                            {
                                Title = (string?)x.Element(ns + "title") ?? "(no title)",
                                Summary = (string?)x.Element(ns + "summary") ?? (string?)x.Element(ns + "content") ?? string.Empty,
                                Link = link
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to preview RSS/Atom for Rss.Id={Id}", Id);
            }

            return Page();
        }
    }
}
