using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System;
using System.Collections.Generic;

namespace digioz.Portal.Web.Pages.Shared.Components.StoreMenu
{
    public class StoreMenuViewComponent : ViewComponent
    {
        private readonly IProductCategoryService _productCategoryService;
        private readonly IPluginService _pluginService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "StoreMenu";

        public StoreMenuViewComponent(IProductCategoryService productCategoryService, IPluginService pluginService, IMemoryCache cache)
        {
            _productCategoryService = productCategoryService;
            _pluginService = pluginService;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out digioz.Portal.Bo.ViewModels.StoreMenuViewModel storeMenu))
            {
                storeMenu = new digioz.Portal.Bo.ViewModels.StoreMenuViewModel
                {
                    ProductCategories = _productCategoryService.GetAll().ToList(),
                    IsEnabled = _pluginService.GetAll().Any(x => x.Name == "Store" && x.IsEnabled)
                };

                _cache.Set(CacheKey, storeMenu, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            return View(storeMenu);
        }
    }
}
