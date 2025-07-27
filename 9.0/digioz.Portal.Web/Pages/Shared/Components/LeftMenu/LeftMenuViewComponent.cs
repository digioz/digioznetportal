using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace digioz.Portal.Web.Pages.Shared.Components.LeftMenu
{
    public class LeftMenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "LeftMenu";

        public LeftMenuViewComponent(IMenuService menuService, IMemoryCache cache)
        {
            _menuService = menuService;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out List<digioz.Portal.Bo.Menu> leftMenu))
            {
                leftMenu = _menuService.GetAll()
                    .Where(x => x.Location == "LeftMenu" && x.Visible == true)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
                _cache.Set(CacheKey, leftMenu, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            return View(leftMenu);
        }
    }
}
