using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digiozPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class RoleManagerController : Controller
    {
        private readonly ILogic<AspNetRoles> _roleLogic;
        private readonly RoleManager<ExtendedIdentityRole> _roleManager;

        public RoleManagerController(
            ILogic<AspNetRoles> roleLogic,
            RoleManager<ExtendedIdentityRole> roleManager
        ) {
            _roleLogic = roleLogic;
            _roleManager = roleManager;
        }

        public IActionResult Index() {
            var models = _roleLogic.GetAll(); 

            return View(models);
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id", "Name")] AspNetRoles role) {
            if (ModelState.IsValid) {
                role.Id = Guid.NewGuid().ToString();
                role.NormalizedName = role.Name;
                role.ConcurrencyStamp = string.Empty;
                _roleLogic.Add(role);

                return RedirectToAction("Index");
            }

            return View(role);
        }

        public IActionResult Delete(string id) {
            var role = _roleLogic.Get(id);

            if (role != null) {
                _roleLogic.Delete(role);

                return RedirectToAction("Index");
            }
                
            return View("Index");
        }

    }
}
