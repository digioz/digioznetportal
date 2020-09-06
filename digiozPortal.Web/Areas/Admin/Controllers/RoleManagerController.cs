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
            // Identity Library does not have 
            // _roleManager.Roles implemented;
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
                // Option 1
                role.Id = Guid.NewGuid().ToString();
                role.NormalizedName = role.Name;
                role.ConcurrencyStamp = string.Empty;
                _roleLogic.Add(role);

                // Option 2
                //var result = await _roleManager.CreateAsync(role);

                //if (result.Succeeded) {
                //    return RedirectToAction("Index");
                //} 
                //else {
                //    ViewBag.Errors = result.Errors;
                //}

                return RedirectToAction("Index");
            }

            return View(role);
        }

        public IActionResult Delete(string id) {
            // Option 1
            var role = _roleLogic.Get(id);

            if (role != null) {
                _roleLogic.Delete(role);

                return RedirectToAction("Index");
            }

            // Option 2
            //var role = await _roleManager.FindByIdAsync(id);

            //if (role != null) {
            //    IdentityResult result = await _roleManager.DeleteAsync(role);

            //    if (result.Succeeded) {
            //        return RedirectToAction("Index");
            //    }
            //    else {
            //        ModelState.AddModelError("", result.Errors.ToString());
            //    }
            //} 
            //else {
            //    ModelState.AddModelError("", "No role found");
            //}
                
            return View("Index");
        }

    }
}
