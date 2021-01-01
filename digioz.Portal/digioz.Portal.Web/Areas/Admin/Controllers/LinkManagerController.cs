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
    public class LinkManagerController : Controller
    {
        private readonly ILogger<LinkManagerController> _logger;
        private readonly ILogic<Link> _linkLogic;
        private readonly ILogic<LinkCategory> _linkCategoryLogic;

        public LinkManagerController(
            ILogger<LinkManagerController> logger,
            ILogic<Link> linkLogic,
            ILogic<LinkCategory> linkCategoryLogic
        ) {
            _logger = logger;
            _linkLogic = linkLogic;
            _linkCategoryLogic = linkCategoryLogic;
        }

        // GET: /Admin/LinkManager/
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> List()
        {
            var models = _linkLogic.GetAll();

            return View(models);
        }

        // GET: /Admin/LinkManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var link = _linkLogic.Get(id);
            ViewBag.LinkCategory = _linkCategoryLogic.GetAll().Where(x => x.Id == link.LinkCategory).SingleOrDefault();

            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        // GET: /Admin/LinkManager/Create
        public async Task<IActionResult> Create()
        {
            var linkCategories = _linkCategoryLogic.GetAll(); 
            ViewBag.LinkCategory = new SelectList(linkCategories, "Id", "Name");

            return View();
        }

        // POST: /Admin/LinkManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id", "Name", "Url", "Description", "Category", "Visible", "Timestamp", "LinkCategory")] Link link)
        {
            if (ModelState.IsValid)
            {
                link.Timestamp = DateTime.Now;
                _linkLogic.Add(link);

                return RedirectToAction("List");
            }

            var linkCategories = _linkCategoryLogic.GetAll();
            ViewBag.LinkCategory = new SelectList(linkCategories, "Id", "Name", link.LinkCategory);

            return View(link);
        }

        // GET: /Admin/LinkManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var link = _linkLogic.Get(id);

            if (link == null)
            {
                return NotFound();
            }

            var linkCategories = _linkCategoryLogic.GetAll();
            ViewBag.LinkCategory = new SelectList(linkCategories, "Id", "Name", link.LinkCategory);

            return View(link);
        }

        // POST: /Admin/LinkManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Url", "Description", "Category", "Visible", "Timestamp", "LinkCategory")] Link link)
        {
            if (ModelState.IsValid)
            {
                _linkLogic.Edit(link);

                return RedirectToAction("List");
            }

            var linkCategories = _linkCategoryLogic.GetAll();
            ViewBag.LinkCategory = new SelectList(linkCategories, "Id", "Name", link.LinkCategory);

            return View(link);
        }

        // GET: /Admin/LinkManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var link = _linkLogic.Get(id);
            ViewBag.LinkCategory = _linkCategoryLogic.GetAll().Where(x => x.Id == link.LinkCategory).SingleOrDefault();

            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        // POST: /Admin/LinkManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var link = _linkLogic.Get(id);
            _linkLogic.Delete(link);

            return RedirectToAction("List");
        }
    }
}
