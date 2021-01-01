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

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class AnnouncementManagerController : Controller
    {
        private readonly ILogic<Announcement> _announcementLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;

        public AnnouncementManagerController(
            ILogic<Announcement> announcementLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic
        )
        {
            _announcementLogic = announcementLogic;
            _configLogic = configLogic;
            _userLogic = userLogic;
        }

        // GET: Admin/AnnouncementManager
        public async Task<IActionResult> Index()
        {
            var models = _announcementLogic.GetAll();

            return View(models);
        }

        // GET: Admin/AnnouncementManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = _announcementLogic.Get(id.GetValueOrDefault());
            ViewBag.UserName = _userLogic.Get(announcement.UserId).UserName;

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // GET: Admin/AnnouncementManager/Create
        public async Task<IActionResult> Create()
        {
            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];

            return View();
        }

        // POST: Admin/AnnouncementManager/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Title,Body,Visible,Timestamp")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                announcement.Timestamp = DateTime.Now;

                _announcementLogic.Add(announcement);

                return RedirectToAction(nameof(Index));
            }

            return View(announcement);
        }

        // GET: Admin/AnnouncementManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = _announcementLogic.Get(id.GetValueOrDefault());
            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Admin/AnnouncementManager/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Title,Body,Visible,Timestamp")] Announcement announcement)
        {
            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var announcementDb = _announcementLogic.Get(announcement.Id);

                announcementDb.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                announcementDb.Timestamp = DateTime.Now;

                announcementDb.Title = announcement.Title;
                announcementDb.Body = announcement.Body;
                announcementDb.Visible = announcement.Visible;

                _announcementLogic.Edit(announcementDb);

                return RedirectToAction("Index");
            }

            var configKeys = _configLogic.GetConfig();
            ViewBag.TinyMCEApiKey = configKeys["TinyMCEApiKey"];

            return View(announcement);
        }

        // GET: Admin/AnnouncementManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = _announcementLogic.Get(id.GetValueOrDefault());
            ViewBag.UserName = _userLogic.Get(announcement.UserId).UserName;

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Admin/AnnouncementManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = _announcementLogic.Get(id);
            _announcementLogic.Delete(announcement);

            return RedirectToAction(nameof(Index));
        }

    }
}
