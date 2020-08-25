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
    public class LinkCategoryManagerController : Controller
    {
        private readonly ILogger<LinkCategoryManagerController> _logger;
        private readonly ILogic<LinkCategory> _linkCategoryLogic;

        public LinkCategoryManagerController(
            ILogger<LinkCategoryManagerController> logger,
            ILogic<LinkCategory> linkCategoryLogic
        ) {
            _logger = logger;
            _linkCategoryLogic = linkCategoryLogic;
        }

        public ActionResult Index()
        {
            var models = _linkCategoryLogic.GetAll();
            return View(models);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var linkCategory = _linkCategoryLogic.Get(id);

            if (linkCategory == null)
            {
                return NotFound();
            }

            return View(linkCategory);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ID", "Name", "Visible", "Timestamp")] LinkCategory linkCategory)
        {
            if (ModelState.IsValid)
            {
                linkCategory.Timestamp = DateTime.Now;
                _linkCategoryLogic.Add(linkCategory);

                return RedirectToAction("Index");
            }

            return View(linkCategory);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var linkCategory = _linkCategoryLogic.Get(id);
            linkCategory.Timestamp = DateTime.Now;

            if (linkCategory == null)
            {
                return NotFound();
            }

            return View(linkCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id", "Name", "Visible", "Timestamp")] LinkCategory linkCategory)
        {
            linkCategory.Timestamp = DateTime.Now;

            if (ModelState.IsValid)
            {
                _linkCategoryLogic.Edit(linkCategory); 

                return RedirectToAction("Index");
            }

            return View(linkCategory);
        }

        // GET: Admin/LinkCategoryManager/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var linkCategory = _linkCategoryLogic.Get(id);

            if (linkCategory == null)
            {
                return NotFound();
            }

            return View(linkCategory);
        }

        // POST: Admin/LinkCategoryManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var linkCategory = _linkCategoryLogic.Get(id);
            _linkCategoryLogic.Delete(linkCategory);

            return RedirectToAction("Index");
        }
    }
}
