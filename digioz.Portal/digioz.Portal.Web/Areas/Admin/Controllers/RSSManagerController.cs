using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;


namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class RSSManagerController : Controller
    {
        private readonly ILogger<RSSManagerController> _logger;
        private readonly ILogic<Rss> _rssLogic;

        public RSSManagerController(
            ILogger<RSSManagerController> logger,
            ILogic<Rss> rssLogic
        )
        {
            _logger = logger;
            _rssLogic = rssLogic;
        }

        // GET: Admin/RSSManager
        public async Task<IActionResult> Index()
        {
            var rssList = _rssLogic.GetAll();

            return View(rssList);
        }

        // GET: Admin/RSSManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Rss rSS = _rssLogic.Get(Convert.ToInt32(id));

            if (rSS == null)
            {
                return NotFound();
            }

            return View(rSS);
        }

        // GET: Admin/RSSManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/RSSManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Url,MaxCount,Timestamp")] Rss rss)
        {
            rss.Timestamp = DateTime.Now;

            if (ModelState.IsValid)
            {
                _rssLogic.Add(rss);
                return RedirectToAction("Index");
            }

            return View(rss);
        }

        // GET: Admin/RSSManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Rss rss = _rssLogic.Get(Convert.ToInt32(id));

            if (rss == null)
            {
                return NotFound();
            }

            return View(rss);
        }

        // POST: Admin/RSSManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Url,MaxCount,Timestamp")] Rss rss)
        {
            var rssDb = _rssLogic.Get(rss.Id);
            
            rssDb.Name = rss.Name;
            rssDb.Url = rss.Url;
            rssDb.MaxCount = rss.MaxCount;
            rssDb.Timestamp = DateTime.Now;

            if (ModelState.IsValid)
            {
                _rssLogic.Edit(rssDb);

                return RedirectToAction("Index");
            }

            return View(rss);
        }

        // GET: Admin/RSSManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Rss rss = _rssLogic.Get(Convert.ToInt32(id));

            if (rss == null)
            {
                return NotFound();
            }

            return View(rss);
        }

        // POST: Admin/RSSManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Rss rss = _rssLogic.Get(Convert.ToInt32(id));

            if (rss != null)
            {
                _rssLogic.Delete(rss);
            }

            return RedirectToAction("Index");
        }
    }
}
