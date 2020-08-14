using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using digiozPortal.BLL;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace digiozPortal.Web.Controllers
{
    public class CommentsController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        ICommentLogic _commentLogic;
        ILogic<CommentLike> _commentLikeLogic;
        ILogic<Config> _configLogic;

        public CommentsController(
            ILogger<HomeController> logger,
            ICommentLogic commentLogic,
            ILogic<CommentLike> commentLike,
            ILogic<Config> configLogic
        ) {
            _logger = logger;
            _commentLogic = commentLogic;
            _commentLikeLogic = commentLike;
            _configLogic = configLogic;
        }

        // GET: Comments
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(int referenceId, string referenceType)
        {
            var comments = _commentLogic.GetCommentPostsByReference(referenceId, referenceType).OrderBy(x => x.ModifiedDate);
            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            //return View(comments);
            return PartialView("List", comments);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(IFormCollection form)
        {
            if (form != null && form["referenceId"].ToString() != string.Empty && form["referenceType"].ToString() != string.Empty && form["comment"].ToString() != string.Empty)
            {
                var guid = Guid.NewGuid();

                Comment comment = new Comment()
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
        public ActionResult Like(string id)
        {
            // Add the Like to the Comment
            var commentLike = new CommentLike() {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CommentId = id
            };

            _commentLikeLogic.Add(commentLike);

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}