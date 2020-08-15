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

namespace digiozPortal.Web.ViewComponents
{
    public class CommentListViewComponent : ViewComponent
    {
        //ICommentLogic _commentLogic;
        ILogic<Comment> _commentLogic;

        public CommentListViewComponent(ILogic<Comment> commentLogic  ) {
            _commentLogic = commentLogic;
       }

        public IViewComponentResult Invoke(int referenceId, string referenceType) {
            // var comments = _commentLogic.GetCommentPostsByReference(referenceId, referenceType).OrderBy(x => x.ModifiedDate);
            var comments = _commentLogic.GetAll();

            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            return View(comments);
        }
    }
}
