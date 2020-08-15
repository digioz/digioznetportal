using System.Linq;
using digiozPortal.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace digiozPortal.Web.ViewComponents
{
    public class CommentListViewComponent : ViewComponent
    {
        ICommentLogic _commentLogic;
        //ILogic<Comment> _commentLogic;

        public CommentListViewComponent(ICommentLogic commentLogic  ) {
            _commentLogic = commentLogic;
       }

        public IViewComponentResult Invoke(int referenceId, string referenceType) {
            var comments = _commentLogic.GetCommentPostsByReference(referenceId, referenceType).OrderBy(x => x.ModifiedDate);
            //var comments = _commentLogic.GetAll();

            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            return View(comments);
        }
    }
}
