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

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class CommentConfigManagerController : Controller
    {
        private readonly ILogic<CommentConfig> _commentConfigLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Announcement> _announcementLogic;
        private readonly ILogic<Page> _pageLogic;

        public CommentConfigManagerController(
            ILogic<CommentConfig> commentConfigLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic,
            ILogic<Announcement> announcementLogic,
            ILogic<Page> pageLogic
        )
        {
            _commentConfigLogic = commentConfigLogic;
            _configLogic = configLogic;
            _userLogic = userLogic;
            _announcementLogic = announcementLogic;
            _pageLogic = pageLogic;
        }

        private List<SelectListItem> GetReferenceTypeList()
        {
            List<SelectListItem> referenceTypeList = new List<SelectListItem>();
            referenceTypeList.Add(new SelectListItem() { Text = "Page", Value = "Page" });
            referenceTypeList.Add(new SelectListItem() { Text = "Announcement", Value = "Announcement" });

            return referenceTypeList;
        }

        // GET: Admin/CommentConfigManager
        public async Task<IActionResult> Index()
        {
            var commentConfigs = _commentConfigLogic.GetAll();

            return View(commentConfigs);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.ReferenceTypes = GetReferenceTypeList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnableCommentsViewModel model)
        {
            if (string.IsNullOrEmpty(model.EnableReference) || string.IsNullOrEmpty(model.ReferenceTypes))
            {
                ModelState.AddModelError("Error", "Please select a Reference Type and Value");
            }

            if (ModelState.IsValid)
            {
                // Populate Object from View Model
                CommentConfig commentConfig = new CommentConfig();
                commentConfig.Id = Guid.NewGuid().ToString();
                commentConfig.ReferenceId = model.EnableReference;
                commentConfig.ReferenceType = model.ReferenceTypes;
                commentConfig.Visible = model.Visible;
                commentConfig.Timestamp = DateTime.Now;

                if (model.ReferenceTypes == "Page")
                {
                    commentConfig.ReferenceTitle = _pageLogic.Get(Convert.ToInt32(model.EnableReference)).Title;
                }
                else if (model.ReferenceTypes == "Announcement")
                {
                    commentConfig.ReferenceTitle = _announcementLogic.Get(Convert.ToInt32(model.EnableReference)).Title;
                }

                _commentConfigLogic.Add(commentConfig);

                return RedirectToAction("Index");
            }

            ViewBag.ReferenceTypes = GetReferenceTypeList();

            return View();
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var commentConfig = _commentConfigLogic.Get(id);
            var commentConfigVM = new EnableCommentsViewModel
            {
                Id = commentConfig.Id,
                EnableReference = commentConfig.ReferenceId,
                ReferenceTypes = commentConfig.ReferenceType,
                Visible = commentConfig.Visible
            };

            ViewBag.ReferenceTypes = GetReferenceTypeList();

            return View(commentConfigVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EnableCommentsViewModel model)
        {
            if (string.IsNullOrEmpty(model.EnableReference) || string.IsNullOrEmpty(model.ReferenceTypes))
            {
                ModelState.AddModelError("Error", "Please select a Reference Type and Value");
            }

            if (ModelState.IsValid)
            {
                // Populate Object from View Model
                var commentConfig = _commentConfigLogic.Get(model.Id);
                commentConfig.ReferenceId = model.EnableReference;
                commentConfig.ReferenceType = model.ReferenceTypes;
                commentConfig.Visible = model.Visible;
                commentConfig.Timestamp = DateTime.Now;

                if (model.ReferenceTypes == "Page")
                {
                    commentConfig.ReferenceTitle = _pageLogic.Get(Convert.ToInt32(model.EnableReference)).Title;
                }
                else if (model.ReferenceTypes == "Announcement")
                {
                    commentConfig.ReferenceTitle = _announcementLogic.Get(Convert.ToInt32(model.EnableReference)).Title;
                }

                _commentConfigLogic.Edit(commentConfig);

                return RedirectToAction("Index");
            }

            ViewBag.ReferenceTypes = GetReferenceTypeList();

            return View(model);
        }

        public async Task<IActionResult> GetEnableReferences(string referenceType)
        {
            List<SelectListItem> referenceTypeListItems = new List<SelectListItem>();

            if (referenceType == "Page")
            {
                var pages = _pageLogic.GetAll();

                foreach (var item in pages)
                {
                    referenceTypeListItems.Add(new SelectListItem() { Text = item.Title, Value = item.Id.ToString() });
                }

                ViewBag.EnableReference = referenceTypeListItems;
            }
            else if (referenceType == "Announcement")
            {
                var announcements = _announcementLogic.GetAll();

                foreach (var item in announcements)
                {
                    referenceTypeListItems.Add(new SelectListItem() { Text = item.Title, Value = item.Id.ToString() });
                }

                ViewBag.EnableReference = referenceTypeListItems;
            }

            return Json(referenceTypeListItems);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            // Fetch Info
            var commentConfig = _commentConfigLogic.Get(id); 
            var commentConfigVM = new EnableCommentsViewModel
            {
                Id = commentConfig.Id,
                EnableReference = commentConfig.ReferenceId,
                ReferenceTypes = commentConfig.ReferenceType,
                Visible = commentConfig.Visible
            };

            if (commentConfig == null)
            {
                return RedirectToAction("Index");
            }

            return View(commentConfigVM);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            // Fetch Record
            var commentConfig = _commentConfigLogic.Get(id); 
            var commentConfigVM = new EnableCommentsViewModel
            {
                Id = commentConfig.Id,
                EnableReference = commentConfig.ReferenceId,
                ReferenceTypes = commentConfig.ReferenceType,
                Visible = commentConfig.Visible
            };

            if (commentConfig == null)
            {
                return RedirectToAction("Index");
            }

            return View(commentConfigVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var model = _commentConfigLogic.Get(id);
            _commentConfigLogic.Delete(model);

            return RedirectToAction("Index");
        }
    }
}