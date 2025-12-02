using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.RssFeed
{
    public class IndexModel : PageModel
    {
        private readonly IRssService _rssService;
        private readonly IPluginService _pluginService;
        public IndexModel(IRssService rssService, IPluginService pluginService)
        {
            _rssService = rssService;
            _pluginService = pluginService;
        }

        public List<FeedRowViewModel> Items { get; private set; } = new();
        public bool Enabled { get; private set; }

        public void OnGet()
        {
            Enabled = _pluginService.GetByName("RSSFeed")?.IsEnabled ?? false;
            if (!Enabled) return;

            var feeds = _rssService.GetAll();
            Items = feeds.Select(f => new FeedRowViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Url = f.Url,
                MaxCount = f.MaxCount
            }).ToList();
        }
    }
}
