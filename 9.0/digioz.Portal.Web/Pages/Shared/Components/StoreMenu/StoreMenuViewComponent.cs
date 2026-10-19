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
        private const string CategoriesCacheKey = "StoreMenu.Categories";

        public Task<IViewComponentResult> InvokeAsync()
        {
            // Always compute plugin enabled state fresh to reflect toggles immediately
            bool isEnabled = _pluginService
                .GetAll()
                .Any(x => string.Equals(x.Name, "Store", StringComparison.OrdinalIgnoreCase) && x.IsEnabled);

            // Cache only the categories (low churn, independent of plugin toggle)
            if (!_cache.TryGetValue(CategoriesCacheKey, out List<digioz.Portal.Bo.ProductCategory>? categories) || categories == null)
            {
                categories = _productCategoryService.GetAll().ToList();
                _cache.Set(CategoriesCacheKey, categories, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(15) });
            }

            var storeMenu = new digioz.Portal.Bo.ViewModels.StoreMenuViewModel
            {
                ProductCategories = categories,
                IsEnabled = isEnabled
            };

            return Task.FromResult<IViewComponentResult>(View(storeMenu));
        }
    }
}
