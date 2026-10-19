using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace digioz.Portal.Web.Pages.Shared.Components.TopMenu
{
    public class TopMenuViewComponent(IMenuService menuService, IMemoryCache cache) : ViewComponent
    {
        private readonly IMenuService _menuService = menuService;
        private readonly IMemoryCache _cache = cache;
        private const string CacheKey = "TopMenu";

        public Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out List<digioz.Portal.Bo.Menu>? topMenu) || topMenu == null)
            {
                topMenu = _menuService.GetAll()
                    .Where(x => x.Location == "TopMenu" && x.Visible)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
                _cache.Set(CacheKey, topMenu, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(15) });
            }
            return Task.FromResult<IViewComponentResult>(View(topMenu));
        }
    }
}
