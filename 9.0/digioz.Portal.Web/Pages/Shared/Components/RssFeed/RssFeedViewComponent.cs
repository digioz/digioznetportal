using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Shared.Components.RssFeed
{
    public class RssFeedViewComponent(IRssService rssService, IPluginService pluginService, IMemoryCache cache) : ViewComponent
    {
        private readonly IRssService _rssService = rssService;
        private readonly IPluginService _pluginService = pluginService;
        private readonly IMemoryCache _cache = cache;
        private const string CacheKey = "VC_RssFeed";

        public Task<IViewComponentResult> InvokeAsync()
        {
            var plugin = _pluginService.GetByName("RSSFeed");
            if (plugin == null || !plugin.IsEnabled)
            {
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            if (!_cache.TryGetValue(CacheKey, out List<RssFeedPreviewViewModel>? model) || model == null)
            {
                var feeds = _rssService.GetAll() ?? new List<digioz.Portal.Bo.Rss>();
                model = new List<RssFeedPreviewViewModel>();
                foreach (var feed in feeds)
                {
                    if (string.IsNullOrWhiteSpace(feed.Url)) continue;
                    var items = LoadTopItems(feed.Url, Math.Min(5, Math.Max(1, feed.MaxCount)));
                    model.Add(new RssFeedPreviewViewModel { Rss = feed, Items = items });
                }
                _cache.Set(CacheKey, model, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }

            // Ensure non-null model
            model ??= new List<RssFeedPreviewViewModel>();
            return Task.FromResult<IViewComponentResult>(View("Default", model));
        }

        private static List<RssItemViewModel> LoadTopItems(string url, int max)
        {
            var list = new List<RssItemViewModel>();
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    MaxCharactersFromEntities = 1024,
                    MaxCharactersInDocument = 1024 * 1024 // 1MB
                };
                using var reader = XmlReader.Create(url, settings);
                var doc = XDocument.Load(reader, LoadOptions.None);
                var root = doc.Root;
                if (root == null) return list;
                if (root.Name.LocalName.Equals("rss", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var x in doc.Descendants("item").Take(max))
                    {
                        var link = (string?)x.Element("link") ?? string.Empty;
                        if (!StringUtils.IsSafeHttpUrl(link)) link = string.Empty;
                        list.Add(new RssItemViewModel
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
                    foreach (var x in doc.Descendants(ns + "entry").Take(max))
                    {
                        var link = x.Elements(ns + "link").FirstOrDefault()?.Attribute("href")?.Value ?? string.Empty;
                        if (!StringUtils.IsSafeHttpUrl(link)) link = string.Empty;
                        list.Add(new RssItemViewModel
                        {
                            Title = (string?)x.Element(ns + "title") ?? "(no title)",
                            Summary = (string?)x.Element(ns + "summary") ?? (string?)x.Element(ns + "content") ?? string.Empty,
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
