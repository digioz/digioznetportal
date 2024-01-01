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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RoleManagerController(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager
        ) {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index() {
            var models = _roleManager.Roles.ToList();

            return View(models);
        }

        public async Task<IActionResult> Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync([Bind("Id", "Name")] IdentityRole role) {
            if (ModelState.IsValid) {

                await _roleManager.CreateAsync(role);
                return RedirectToAction("Index");
            }

            return View(role);
        }

        public async Task<IActionResult> DeleteAsync(string id) {
            var role = await _roleManager.FindByIdAsync(id);

            if (role != null) {
                // Remove all user roles first
                var usersOfRole = await _userManager.GetUsersInRoleAsync(role.Name);

                foreach (var user in usersOfRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }

                // Remove Role
                await _roleManager.DeleteAsync(role);

                return RedirectToAction("Index");
            }
                
            return View("Index");
        }

    }
}
