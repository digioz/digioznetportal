using System;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Comments
{
    public class LikeModel : PageModel
    {
        private readonly ICommentService _commentService;
        private readonly IMemoryCache _cache;

        public LikeModel(ICommentService commentService, IMemoryCache cache)
        {
            _commentService = commentService;
            _cache = cache;
        }

        public IActionResult OnPost(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");

            var comment = _commentService.GetAll().FirstOrDefault(c => c.Id == id);
            if (comment != null)
            {
                comment.Likes += 1;
                _commentService.Update(comment);

                // Invalidate cache for its page (ReferenceType holds page path)
                if (!string.IsNullOrWhiteSpace(comment.ReferenceType))
                {
                    _cache.Remove("CommentsMenu_" + comment.ReferenceType);
                }
            }

            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }
    }
}
