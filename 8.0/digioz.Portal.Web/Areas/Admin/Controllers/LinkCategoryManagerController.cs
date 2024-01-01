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
    public class LinkCategoryManagerController : Controller
    {
        private readonly ILogic<LinkCategory> _linkCategoryLogic;

        public LinkCategoryManagerController(
            ILogic<LinkCategory> linkCategoryLogic
        ) {
            _linkCategoryLogic = linkCategoryLogic;
        }

        public async Task<IActionResult> Index()
        {
            var models = _linkCategoryLogic.GetAll();
            return View(models);
        }

        public async Task<IActionResult> Details(int? id)
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

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id", "Name", "Visible", "Timestamp")] LinkCategory linkCategory)
        {
            if (ModelState.IsValid)
            {
                linkCategory.Timestamp = DateTime.Now;
                _linkCategoryLogic.Add(linkCategory);

                return RedirectToAction("Index");
            }

            return View(linkCategory);
        }

        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Visible", "Timestamp")] LinkCategory linkCategory)
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
        public async Task<IActionResult> Delete(int? id)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linkCategory = _linkCategoryLogic.Get(id);
            _linkCategoryLogic.Delete(linkCategory);

            return RedirectToAction("Index");
        }
    }
}
