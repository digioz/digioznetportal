using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Collections.Generic;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Utilities;

namespace digioz.Portal.Web.Pages.Shared.Components.WhoIsOnlineMenu
{
    public class WhoIsOnlineMenuViewComponent : ViewComponent
    {
        private readonly IPluginService _pluginService;
        private readonly IVisitorSessionService _visitorSessionService;
        private readonly IVisitorInfoService _visitorInfoService;
        private readonly IProfileService _profileService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "WhoIsOnlineMenu";

        public WhoIsOnlineMenuViewComponent(IPluginService pluginService, 
                                            IVisitorSessionService visitorSessionService,
                                            IVisitorInfoService visitorInfoService,
                                            IProfileService profileService,
                                            IMemoryCache cache)
        {
            _pluginService = pluginService;
            _visitorSessionService = visitorSessionService;
            _visitorInfoService = visitorInfoService;
            _profileService = profileService;
            _cache = cache;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out WhoIsOnlineViewModel? whoisOnline) || whoisOnline == null)
            {
                whoisOnline = new WhoIsOnlineViewModel();

                var configWhoIsOnline = _pluginService.GetAll().Where(x => x.Name == "WhoIsOnline").SingleOrDefault();
                var latestVisitors = _visitorSessionService.GetAllGreaterThan(DateTime.Now.AddMinutes(-10)).ToList();
                var visitorRegistered = latestVisitors.Where(x => x.Username != null).ToList();
                visitorRegistered = System.Linq.Enumerable.DistinctBy(visitorRegistered, x => x.Username).ToList();

                // Load profile DisplayName for each visitor and create a new list with enriched data
                var enrichedVisitors = new List<VisitorSession>();
                foreach (var visitor in visitorRegistered)
                {
                    // Load the profile by email (VisitorSession.Username contains the email)
                    var profile = _profileService.GetByEmail(visitor.Username);
                    
                    // Create a new VisitorSession object with the Profile data
                    var enrichedVisitor = new VisitorSession
                    {
                        Id = visitor.Id,
                        IpAddress = visitor.IpAddress,
                        PageUrl = visitor.PageUrl,
                        SessionId = visitor.SessionId,
                        Username = visitor.Username,
                        DateCreated = visitor.DateCreated,
                        DateModified = visitor.DateModified,
                        Profile = profile  // Assign the loaded profile
                    };
                    enrichedVisitors.Add(enrichedVisitor);
                }

                // Get bot visitors from VisitorInfo table
                var recentVisitorInfo = _visitorInfoService.GetAllGreaterThan(DateTime.Now.AddMinutes(-10));
                var botVisitors = recentVisitorInfo
                    .Where(v => BotHelper.IsBot(v.UserAgent))
                    .GroupBy(v => v.IpAddress)
                    .Select(g => new BotVisitorViewModel
                    {
                        IpAddress = g.Key ?? "Unknown",
                        UserAgent = g.First().UserAgent ?? string.Empty,
                        BotName = BotHelper.ExtractBotName(g.First().UserAgent ?? string.Empty)
                    })
                    .ToList();

                whoisOnline.VisitorCount = latestVisitors.Count;
                whoisOnline.RegisteredVisitors = enrichedVisitors;
                whoisOnline.Bots = botVisitors;

                whoisOnline.WhoIsOnlineEnabled = configWhoIsOnline != null && configWhoIsOnline.IsEnabled;

                _cache.Set(CacheKey, whoisOnline, new MemoryCacheEntryOptions().SetSlidingExpiration(System.TimeSpan.FromMinutes(1)));
            }
            return Task.FromResult<IViewComponentResult>(View(whoisOnline));
        }
    }
}