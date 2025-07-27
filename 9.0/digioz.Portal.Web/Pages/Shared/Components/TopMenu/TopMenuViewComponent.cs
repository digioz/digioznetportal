using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace digioz.Portal.Web.Pages.Shared.Components.TopMenu
{
    public class TopMenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "TopMenu";

        public TopMenuViewComponent(IMenuService menuService, IMemoryCache cache)
        {
            _menuService = menuService;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out List<digioz.Portal.Bo.Menu> topMenu))
            {
                topMenu = _menuService.GetAll()
                    .Where(x => x.Location == "TopMenu" && x.Visible == true)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
                _cache.Set(CacheKey, topMenu, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            return View(topMenu);
        }
    }
}
