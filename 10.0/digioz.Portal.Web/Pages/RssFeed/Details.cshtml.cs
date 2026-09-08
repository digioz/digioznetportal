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

namespace digioz.Portal.Web.Pages.RssFeed
{
    public class DetailsModel : PageModel
    {
        private readonly IRssService _rssService;
        private readonly IPluginService _pluginService;
        public DetailsModel(IRssService rssService, IPluginService pluginService)
        {
            _rssService = rssService;
            _pluginService = pluginService;
        }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public bool Enabled { get; private set; }
        public string FeedName { get; private set; } = string.Empty;
        public List<FeedItemViewModel> Items { get; private set; } = new();

        public IActionResult OnGet()
        {
            Enabled = _pluginService.GetByName("RSSFeed")?.IsEnabled ?? false;
            if (!Enabled) return Page();

            var feed = _rssService.Get(Id);
            if (feed == null) return NotFound();
            FeedName = feed.Name;

            Items = LoadItems(feed.Url, Math.Max(1, feed.MaxCount));
            return Page();
        }

        private static List<FeedItemViewModel> LoadItems(string url, int max)
        {
            var list = new List<FeedItemViewModel>();
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    MaxCharactersFromEntities = 1024,
                    MaxCharactersInDocument = 1024 * 1024
                };
                using var reader = XmlReader.Create(url, settings);
                var doc = XDocument.Load(reader, LoadOptions.None);
                var root = doc.Root;
                if (root == null) return list;
                if (root.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var x in doc.Descendants("item").Take(max))
                    {
                        var link = (string?)x.Element("link");
                        if (!StringUtils.IsSafeHttpUrl(link)) link = null;
                        list.Add(new FeedItemViewModel
                        {
                            Title = (string?)x.Element("title") ?? "(no title)",
                            Summary = (string?)x.Element("description"),
                            Link = link
                        });
                    }
                }
                else if (root.Name.LocalName.Equals("feed", StringComparison.OrdinalIgnoreCase))
                {
                    XNamespace ns = root.Name.Namespace;
                    foreach (var x in doc.Descendants(ns + "entry").Take(max))
                    {
                        var link = x.Elements(ns + "link").FirstOrDefault()?.Attribute("href")?.Value;
                        if (!StringUtils.IsSafeHttpUrl(link)) link = null;
                        list.Add(new FeedItemViewModel
                        {
                            Title = (string?)x.Element(ns + "title") ?? "(no title)",
                            Summary = (string?)x.Element(ns + "summary") ?? (string?)x.Element(ns + "content"),
                            Link = link
                        });
                    }
                }
            }
            catch
            {
                // ignore
            }
            return list;
        }
    }
}
