using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class RoleManagerController : Controller
    {
        private readonly ILogic<AspNetRole> _roleLogic;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagerController(
            ILogic<AspNetRole> roleLogic,
            RoleManager<IdentityRole> roleManager
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
        public IActionResult Create([Bind("Id", "Name")] AspNetRole role) {
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
