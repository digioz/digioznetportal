using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using digiozPortal.BLL;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digiozPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")] 
    public class PageManagerController : Controller
    {
        private readonly ILogic<Page> _pageLogic;
        private readonly IConfigLogic _configLogic;
        // private readonly ILogic<AspNetUsers> _userLogic;

        public PageManagerController(
            ILogic<Page> pageLogic,
            IConfigLogic configLogic
            // ILogic<AspNetUsers> userLogic
        ) {
            _pageLogic = pageLogic;
            _configLogic = configLogic;
            //_userLogic = userLogic;
        }

        public ActionResult Index()
        {
            var pages = _pageLogic.GetAll();
            return View(pages);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var page = _pageLogic.Get(id.GetValueOrDefault());

            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        public ActionResult Create()
        {
            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ID", "UserID", "Title", "URL", "Body", "Keywords", "Description", "Visible", "Timestamp")] Page page)
        {
            if (ModelState.IsValid)
            {
                page.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                page.Timestamp = DateTime.Now;

                _pageLogic.Add(page);

                return RedirectToAction("Index");
            }

            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];

            return View(page);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var page = _pageLogic.Get(id.GetValueOrDefault());
            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];

            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("ID", "UserID", "Title", "URL", "Body", "Keywords", "Description", "Visible", "Timestamp")] Page page)
        {
            if (ModelState.IsValid)
            {
                var pageDb = _pageLogic.Get(page.ID);
                
                pageDb.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                pageDb.Timestamp = DateTime.Now;

                pageDb.Title = page.Title;
                pageDb.URL = page.URL;
                pageDb.Body = page.Body;
                pageDb.Keywords = page.Keywords;
                pageDb.Description = page.Description;
                pageDb.Visible = page.Visible;
                
                _pageLogic.Edit(pageDb);
                
                return RedirectToAction("Index");
            }

            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];
            return View(page);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var page = _pageLogic.Get(id.GetValueOrDefault());
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var page = _pageLogic.Get(id);
            _pageLogic.Delete(page);
            return RedirectToAction("Index");
        }
    }
}
