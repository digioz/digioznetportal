using System.Linq;
using digioz.Portal.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.ViewComponents
{
    public class CommentListViewComponent : ViewComponent
    {
        ICommentLogic _commentLogic;

        public CommentListViewComponent(ICommentLogic commentLogic  ) {
            _commentLogic = commentLogic;
       }

        public IViewComponentResult Invoke(int referenceId, string referenceType) {
            var comments = _commentLogic.GetCommentPostsByReference(referenceId, referenceType).OrderBy(x => x.ModifiedDate);

            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            return View(comments);
        }
    }
}
