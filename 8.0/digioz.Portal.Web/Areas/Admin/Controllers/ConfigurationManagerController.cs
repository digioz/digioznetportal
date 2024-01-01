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
    public class ConfigurationManagerController : Controller
    {
        private readonly IConfigLogic _configLogic;

        public ConfigurationManagerController(
            IConfigLogic configLogic
        )
        {
            _configLogic = configLogic;
        }

        // GET: /Admin/ConfigurationManager/
        public async Task<IActionResult> Index()
        {
            return View(_configLogic.GetAll());
        }

        // GET: /Admin/ConfigurationManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var config = _configLogic.Get(id);

            if (config == null)
            {
                return NotFound();
            }
            return View(config);
        }

        // GET: /Admin/ConfigurationManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: /Admin/ConfigurationManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ConfigKey,ConfigValue,IsEncrypted")] Config config)
        {
            config.Id = Guid.NewGuid().ToString();

            if (ModelState.IsValid)
            {
                _configLogic.Add(config);

                return RedirectToAction("Index");
            }

            return View(config);
        }

        // GET: /Admin/ConfigurationManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var config = _configLogic.Get(id);

            if (config == null)
            {
                return NotFound();
            }

            return View(config);
        }

        // POST: /Admin/ConfigurationManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,ConfigKey,ConfigValue,IsEncrypted")] Config config)
        {
            if (ModelState.IsValid)
            {
                var configDb = _configLogic.Get(config.Id);
                configDb.ConfigKey = config.ConfigKey;
                configDb.ConfigValue = config.ConfigValue;
                configDb.IsEncrypted = configDb.IsEncrypted;

                _configLogic.Edit(configDb);

                return RedirectToAction("Index");
            }
            return View(config);
        }

        // GET: /Admin/ConfigurationManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var config = _configLogic.Get(id);

            if (config == null)
            {
                return NotFound();
            }

            return View(config);
        }

        // POST: /Admin/ConfigurationManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var config = _configLogic.Get(id);
            _configLogic.Delete(config);
            return RedirectToAction("Index");
        }
    }
}
