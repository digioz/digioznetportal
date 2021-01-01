using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using digioz.Portal.Bll;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Controllers
{
    public class CommentsController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICommentLogic _commentLogic;
        private readonly ILogic<CommentLike> _commentLikeLogic;

        public CommentsController(
            ILogger<HomeController> logger,
            ICommentLogic commentLogic,
            ILogic<CommentLike> commentLike
        ) {
            _logger = logger;
            _commentLogic = commentLogic;
            _commentLikeLogic = commentLike;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> List(int referenceId, string referenceType)
        {
            var comments = _commentLogic.GetCommentPostsByReference(referenceId, referenceType).OrderBy(x => x.ModifiedDate);
            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            return PartialView("List", comments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(IFormCollection form)
        {
            if (form != null && form["referenceId"].ToString() != string.Empty && form["referenceType"].ToString() != string.Empty && form["comment"].ToString() != string.Empty)
            {
                var guid = Guid.NewGuid();

                var comment = new Comment()
                {
                    Id = guid.ToString(),
                    ReferenceId = form["referenceId"].ToString(),
                    ReferenceType = form["referenceType"].ToString(),
                    Body = form["comment"].ToString(),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Likes = 0,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Username = User.Identity.Name
                };

                // Add Comment
                _commentLogic.Add(comment);
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [Authorize]
        public async Task<IActionResult> Like(string id)
        {
            // Add the Like to the Comment
            var commentLike = new CommentLike() {
                Id = Guid.NewGuid().ToString(),
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CommentId = id
            };

            _commentLikeLogic.Add(commentLike);

            // Update Count on Comment Record
            var comment = _commentLogic.Get(id);
            comment.Likes += 1;
            _commentLogic.Edit(comment);

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}