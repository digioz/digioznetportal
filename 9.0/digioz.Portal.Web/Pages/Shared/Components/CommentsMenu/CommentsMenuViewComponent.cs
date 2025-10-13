using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Shared.Components.CommentsMenu
{
    public class CommentsMenuViewComponent : ViewComponent
    {
        private readonly ICommentService _commentService;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "CommentsMenu_";

        public CommentsMenuViewComponent(ICommentService commentService, IMemoryCache cache)
        {
            _commentService = commentService;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync(string referenceId, string referenceType)
        {
            // Pass identifiers to the view (used by forms/links)
            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            var cacheKey = $"{CacheKeyPrefix}{referenceType}_{referenceId}";

            if (!_cache.TryGetValue(cacheKey, out List<Comment> comments))
            {
                comments = _commentService
                    .GetAll()
                    .Where(c => c.ReferenceId == referenceId && c.ReferenceType == referenceType)
                    .OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate)
                    .ToList();

                _cache.Set(cacheKey, comments, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }

            return View(comments);
        }
    }
}
