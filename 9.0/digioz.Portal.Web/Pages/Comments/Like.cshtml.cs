using System;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Comments
{
    [Authorize] // Require authentication
    [ValidateAntiForgeryToken] // Require anti-forgery token
    public class LikeModel : PageModel
    {
        private readonly ICommentService _commentService;
        private readonly IMemoryCache _cache;
        private readonly IUserHelper _userHelper;

        public LikeModel(ICommentService commentService, IMemoryCache cache, IUserHelper userHelper)
        {
            _commentService = commentService;
            _cache = cache;
            _userHelper = userHelper;
        }

        public IActionResult OnPost(string id)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Comment ID is required");
            }

            // Get comment directly instead of loading all comments
            var comment = _commentService.Get(id);
            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            // Get current user ID
            var userName = User.Identity?.Name;
            var currentUserId = !string.IsNullOrEmpty(userName) ? _userHelper.GetUserIdByEmail(userName) : null;

            // Prevent users from liking their own comments
            if (!string.IsNullOrEmpty(currentUserId) && comment.UserId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot like your own comment";
                return RedirectToSafePage(comment.ReferenceType);
            }

            // TODO: Implement a CommentLike table to track who liked what and prevent duplicate likes
            // For now, just increment the counter (this allows duplicate likes from same user)
            comment.Likes += 1;
            _commentService.Update(comment);
            
            // Clear cache for this comment section
            _cache.Remove("CommentsMenu_" + comment.ReferenceType);

            return RedirectToSafePage(comment.ReferenceType);
        }

        /// <summary>
        /// Safely redirects to a page, validating the URL to prevent open redirect attacks
        /// </summary>
        private IActionResult RedirectToSafePage(string? referenceType)
        {
            // Validate referer header exists and is from same site
            var referer = Request.Headers["Referer"].ToString();
            
            if (!string.IsNullOrWhiteSpace(referer))
            {
                // Check if referer is from the same host
                if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
                {
                    var requestHost = Request.Host.Host;
                    if (refererUri.Host.Equals(requestHost, StringComparison.OrdinalIgnoreCase))
                    {
                        // Safe to redirect to referer
                        return Redirect(referer);
                    }
                }
            }

            // Fall back to reference type or home page
            if (!string.IsNullOrWhiteSpace(referenceType) && referenceType.StartsWith("/"))
            {
                return Redirect(referenceType);
            }

            return RedirectToPage("/Index");
        }
    }
}
