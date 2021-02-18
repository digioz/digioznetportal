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
    public class PluginManagerController : Controller
    {
        private readonly ILogic<Plugin> _pluginLogic;

        public PluginManagerController(
            ILogic<Plugin> pluginLogic
        )
        {
            _pluginLogic = pluginLogic;
        }

        // GET: Admin/PluginManager
        public async Task<IActionResult> Index()
        {
            return View(_pluginLogic.GetAll());
        }

        // GET: Admin/PluginManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var plugin = _pluginLogic.Get(id);

            if (plugin == null)
            {
                return NotFound();
            }

            return View(plugin);
        }

        // GET: Admin/PluginManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/PluginManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Dll,IsEnabled")] Plugin plugin)
        {
            if (ModelState.IsValid)
            {
                _pluginLogic.Add(plugin);

                return RedirectToAction("Index");
            }

            return View(plugin);
        }

        // GET: Admin/PluginManager/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var plugin = _pluginLogic.Get(id); 

            if (plugin == null)
            {
                return NotFound();
            }

            return View(plugin);
        }

        // POST: Admin/PluginManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Dll,IsEnabled")] Plugin plugin)
        {
            if (ModelState.IsValid)
            {
                _pluginLogic.Edit(plugin);

                return RedirectToAction("Index");
            }

            return View(plugin);
        }

        // GET: Admin/PluginManager/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var plugin = _pluginLogic.Get(id);

            if (plugin == null)
            {
                return NotFound();
            }

            return View(plugin);
        }

        // POST: Admin/PluginManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plugin = _pluginLogic.Get(id);
            _pluginLogic.Delete(plugin);

            return RedirectToAction("Index");
        }
    }
}
