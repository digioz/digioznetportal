using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Bo;
using digioz.Portal.Dal;
using Microsoft.AspNetCore.Authorization;
using digioz.Portal.Bll.Interfaces;
using System.Security.Claims;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using digioz.Portal.Utilities;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class CommentPostsManagerController : Controller
    {
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Comment> _commentLogic;

        public CommentPostsManagerController(
            ILogic<AspNetUser> userLogic,
            ILogic<Comment> commentLogic
        )
        {
            _userLogic = userLogic;
            _commentLogic = commentLogic;
        }

        // GET: Admin/CommentPostsManager
        public async Task<IActionResult> Index()
        {
            var users = _userLogic.GetAll();
            var comments = _commentLogic.GetAll();
            var commentsVM = new List<CommentPostViewModel>();

            foreach (var item in comments)
            {
                CommentPostViewModel itemVM = new CommentPostViewModel();

                itemVM.Id = item.Id;
                itemVM.ParentId = item.ParentId;
                itemVM.UserId = item.UserId;
                itemVM.Username = item.Username;
                itemVM.Body = item.Body;
                itemVM.CreatedDate = item.CreatedDate;
                itemVM.ModifiedDate = item.ModifiedDate;
                itemVM.Likes = item.Likes;
                itemVM.ReferenceId = item.ReferenceId;
                itemVM.ReferenceType = item.ReferenceType;

                if (item.UserId != null)
                {
                    itemVM.Username = users.Where(x => x.Id == item.UserId).SingleOrDefault().UserName;
                }
                else
                {
                    itemVM.Username = "Anonymous";
                }
                
                itemVM.Body = StringUtils.TruncateDotDotDot(item.Body, 50);

                commentsVM.Add(itemVM);
            }

            commentsVM = commentsVM.OrderByDescending(x => x.ModifiedDate).ToList();

            return View(commentsVM);
        }

        // GET: Admin/CommentPostsManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Comment comment = _commentLogic.Get(id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Admin/CommentPostsManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/CommentPostsManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParentId,UserId,Body,CreatedDate,ModifiedDate,Likes")] Comment model)
        {
            model.Id = Guid.NewGuid().ToString();
            model.CreatedDate = DateTime.Now;
            model.ModifiedDate = DateTime.Now;
            model.Likes = 0;
            if (model.UserId == null)
            {
                model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            if (ModelState.IsValid)
            {
                _commentLogic.Add(model);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Admin/CommentPostsManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Comment comment = _commentLogic.Get(id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Admin/CommentPostsManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,ParentId,UserId,Body,CreatedDate,ModifiedDate,Likes")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                var commentDb = _commentLogic.Get(comment.Id);
                commentDb.ParentId = comment.ParentId;
                commentDb.Body = comment.Body;
                commentDb.ModifiedDate = DateTime.Now;

                _commentLogic.Edit(commentDb);

                return RedirectToAction("Index");
            }
            return View(comment);
        }

        // GET: Admin/CommentPostsManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Comment comment = _commentLogic.Get(id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Admin/CommentPostsManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var model = _commentLogic.Get(id);
            _commentLogic.Delete(model);

            return RedirectToAction("Index");
        }
    }
}
