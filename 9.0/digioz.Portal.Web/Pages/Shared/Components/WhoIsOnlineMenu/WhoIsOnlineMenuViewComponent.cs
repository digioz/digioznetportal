using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using digioz.Portal.Bo.ViewModels;

namespace digioz.Portal.Web.Pages.Shared.Components.WhoIsOnlineMenu
{
    public class WhoIsOnlineMenuViewComponent : ViewComponent
    {
        private readonly IPluginService _pluginService;
        private readonly IVisitorSessionService _visitorSessionService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "WhoIsOnlineMenu";

        public WhoIsOnlineMenuViewComponent(IPluginService pluginService, 
                                            IVisitorSessionService visitorSessionService, 
                                            IMemoryCache cache)
        {
            _pluginService = pluginService;
            _visitorSessionService = visitorSessionService;
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
                visitorRegistered = visitorRegistered.DistinctBy(x => x.Username).ToList();

                whoisOnline.VisitorCount = latestVisitors.Count;
                whoisOnline.RegisteredVisitors = visitorRegistered;

                whoisOnline.WhoIsOnlineEnabled = configWhoIsOnline != null && configWhoIsOnline.IsEnabled;

                _cache.Set(CacheKey, whoisOnline, new MemoryCacheEntryOptions().SetSlidingExpiration(System.TimeSpan.FromMinutes(15)));
            }
            return Task.FromResult<IViewComponentResult>(View(whoisOnline));
        }
    }
}