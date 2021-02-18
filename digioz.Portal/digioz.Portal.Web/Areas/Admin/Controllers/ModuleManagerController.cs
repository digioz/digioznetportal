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
    public class ModuleManagerController : Controller
    {
        private readonly ILogic<Module> _moduleLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Zone> _zoneLogic;

        public ModuleManagerController(
            ILogic<Module> moduleLogic,
            ILogic<AspNetUser> userLogic,
            ILogic<Zone> zoneLogic
        )
        {
            _moduleLogic = moduleLogic;
            _userLogic = userLogic;
            _zoneLogic = zoneLogic;
        }

        // GET: /Admin/ModuleManager/
        public ActionResult Index()
        {
            var modules = _moduleLogic.GetAll(); 

            return View(modules.ToList());
        }

        // GET: /Admin/ModuleManager/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var module = _moduleLogic.Get(id);

            if (module == null)
            {
                return NotFound();
            }

            return View(module);
        }

        // GET: /Admin/ModuleManager/Create
        public ActionResult Create()
        {
            // Get user list
            ViewBag.UserId = new SelectList(_userLogic.GetAll(), "Id", "UserName");

            // Get Menu Locations
            ViewBag.Location = new SelectList(_zoneLogic.GetGeneric(x => x.ZoneType == "Module"), "Name", "Name");

            return View();
        }

        // POST: /Admin/ModuleManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,UserId,Title,Body,Visible,Timestamp,Location,DisplayInBox")] Module module)
        {
            if (ModelState.IsValid)
            {
                module.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                module.Timestamp = DateTime.Now;

                _moduleLogic.Add(module);

                return RedirectToAction("Index");
            }

            // Get user list
            ViewBag.UserId = new SelectList(_userLogic.GetAll(), "Id", "UserName", module.UserId);

            // Get Menu Locations
            ViewBag.Location = new SelectList(_zoneLogic.GetGeneric(x => x.ZoneType == "Module"), "Name", "Name", module.Location);

            return View(module);
        }

        // GET: /Admin/ModuleManager/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var module = _moduleLogic.Get(id);

            if (module == null)
            {
                return NotFound();
            }

            // Get user list
            ViewBag.UserId = new SelectList(_userLogic.GetAll(), "Id", "UserName", module.UserId);

            // Get Menu Locations
            ViewBag.Location = new SelectList(_zoneLogic.GetGeneric(x => x.ZoneType == "Module"), "Name", "Name", module.Location);

            return View(module);
        }

        // POST: /Admin/ModuleManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,UserId,Title,Body,Visible,Timestamp,Location,DisplayInBox")] Module module)
        {
            if (ModelState.IsValid)
            {
                module.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                module.Timestamp = DateTime.Now;

                _moduleLogic.Edit(module);

                return RedirectToAction("Index");
            }

            // Get user list
            ViewBag.UserId = new SelectList(_userLogic.GetAll(), "Id", "UserName", module.UserId);

            // Get Menu Locations
            ViewBag.Location = new SelectList(_zoneLogic.GetGeneric(x => x.ZoneType == "Module"), "Name", "Name", module.Location);

            return View(module);
        }

        // GET: /Admin/ModuleManager/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var module = _moduleLogic.Get(id);

            if (module == null)
            {
                return NotFound();
            }

            return View(module);
        }

        // POST: /Admin/ModuleManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var module = _moduleLogic.Get(id);
            _moduleLogic.Delete(module);

            return RedirectToAction("Index");
        }
    }
}