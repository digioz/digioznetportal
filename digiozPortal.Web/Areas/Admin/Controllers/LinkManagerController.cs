using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digiozPortal.Web.Areas.Admin.Controllers
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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            var models = _linkLogic.GetAll();

            return View(models);
        }

        // GET: /Admin/LinkManager/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var link = _linkLogic.Get(id);
            ViewBag.LinkCategory = _linkCategoryLogic.GetAll().Where(x => x.Id == link.LinkCategoryId).SingleOrDefault();

            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        // GET: /Admin/LinkManager/Create
        public ActionResult Create()
        {
            var linkCategories = _linkCategoryLogic.GetAll(); 
            ViewBag.LinkCategoryID = new SelectList(linkCategories, "Id", "Name");

            return View();
        }

        // POST: /Admin/LinkManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ID", "Name", "URL", "Description", "Category", "Visible", "Timestamp", "LinkCategoryId")] Link link)
        {
            if (ModelState.IsValid)
            {
                link.Timestamp = DateTime.Now;
                _linkLogic.Add(link);

                return RedirectToAction("List");
            }

            var linkCategories = _linkCategoryLogic.GetAll();
            ViewBag.LinkCategoryID = new SelectList(linkCategories, "Id", "Name", link.LinkCategoryId);

            return View(link);
        }

        // GET: /Admin/LinkManager/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.LinkCategoryID = new SelectList(linkCategories, "Id", "Name", link.LinkCategoryId);

            return View(link);
        }

        // POST: /Admin/LinkManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id", "Name", "URL", "Description", "Category", "Visible", "Timestamp", "LinkCategoryId")] Link link)
        {
            if (ModelState.IsValid)
            {
                _linkLogic.Edit(link);

                return RedirectToAction("Index");
            }

            var linkCategories = _linkCategoryLogic.GetAll();
            ViewBag.LinkCategoryID = new SelectList(linkCategories, "Id", "Name", link.LinkCategoryId);

            return View(link);
        }

        // GET: /Admin/LinkManager/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var link = _linkLogic.Get(id);
            ViewBag.LinkCategory = _linkCategoryLogic.GetAll().Where(x => x.Id == link.LinkCategoryId).SingleOrDefault();

            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        // POST: /Admin/LinkManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Link link = _linkLogic.Get(id);
            _linkLogic.Delete(link);

            return RedirectToAction("Index");
        }
    }
}
