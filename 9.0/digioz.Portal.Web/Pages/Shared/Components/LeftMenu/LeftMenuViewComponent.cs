using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace digioz.Portal.Web.Pages.Shared.Components.LeftMenu
{
    public class LeftMenuViewComponent(IMenuService menuService, IMemoryCache cache) : ViewComponent
    {
        private const string CacheKey = "LeftMenu";
        private readonly IMenuService _menuService = menuService;
        private readonly IMemoryCache _cache = cache;

        public Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out List<digioz.Portal.Bo.Menu>? leftMenu) || leftMenu == null)
            {
                leftMenu = _menuService.GetAll()
                    .Where(x => x.Location == "LeftMenu" && x.Visible == true)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
                _cache.Set(CacheKey, leftMenu, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            return Task.FromResult<IViewComponentResult>(View(leftMenu));
        }
    }
}
