using System;
using System.Security.Claims;
using System.Threading.Tasks;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")] 
    public class PageManagerController : Controller
    {
        private readonly ILogic<Page> _pageLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;

        public PageManagerController(
            ILogic<Page> pageLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic
        ) {
            _pageLogic = pageLogic;
            _configLogic = configLogic;
            _userLogic = userLogic;
        }

        public async Task<IActionResult> Index()
        {
            var pages = _pageLogic.GetAll();
            return View(pages);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var page = _pageLogic.Get(id.GetValueOrDefault());
            ViewBag.UserName = _userLogic.Get(page.UserId).UserName;

            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        public async Task<IActionResult> Create()
        {
            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id", "UserId", "Title", "Url", "Body", "Keywords", "Description", "Visible", "Timestamp")] Page page)
        {
            if (ModelState.IsValid)
            {
                page.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                page.Timestamp = DateTime.Now;

                _pageLogic.Add(page);

                return RedirectToAction("Index");
            }

            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];

            return View(page);
        }

        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit([Bind("Id", "UserId", "Title", "Url", "Body", "Keywords", "Description", "Visible", "Timestamp")] Page page)
        {
            if (ModelState.IsValid)
            {
                var pageDb = _pageLogic.Get(page.Id);
                
                pageDb.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                pageDb.Timestamp = DateTime.Now;

                pageDb.Title = page.Title;
                pageDb.Url = page.Url;
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var page = _pageLogic.Get(id.GetValueOrDefault());
            ViewBag.UserName = _userLogic.Get(page.UserId).UserName;

            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = _pageLogic.Get(id);
            _pageLogic.Delete(page);
            return RedirectToAction("Index");
        }
    }
}
