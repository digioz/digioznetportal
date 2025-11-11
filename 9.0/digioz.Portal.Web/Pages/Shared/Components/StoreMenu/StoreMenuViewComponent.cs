using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System;
using System.Collections.Generic;

namespace digioz.Portal.Web.Pages.Shared.Components.StoreMenu
{
    public class StoreMenuViewComponent(
        IProductCategoryService productCategoryService,
        IPluginService pluginService,
        IMemoryCache cache
    ) : ViewComponent
    {
        private readonly IProductCategoryService _productCategoryService = productCategoryService;
        private readonly IPluginService _pluginService = pluginService;
        private readonly IMemoryCache _cache = cache;
        private const string CacheKey = "StoreMenu";

        public Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out digioz.Portal.Bo.ViewModels.StoreMenuViewModel? storeMenu) || storeMenu == null)
            {
                storeMenu = new digioz.Portal.Bo.ViewModels.StoreMenuViewModel
                {
                    ProductCategories = _productCategoryService.GetAll().ToList(),
                    IsEnabled = _pluginService.GetAll().Any(x => x.Name == "Store" && x.IsEnabled)
                };

                _cache.Set(CacheKey, storeMenu, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(15) });
            }
            return Task.FromResult<IViewComponentResult>(View(storeMenu));
        }
    }
}
