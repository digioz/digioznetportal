using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.Web.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace digiozPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class UserManagerController : Controller
    {
        private readonly ILogic<AspNetUsers> _userLogic;

        public UserManagerController(
            ILogic<AspNetUsers> userLogic
        ) 
        {
            _userLogic = userLogic;
        }

        public ActionResult Index() {
            var users = _userLogic.GetAll();

            return View(users);
        }

        [HttpGet]
        [Route("/admin/usermanager/details/{id}")]
        public ActionResult Details(string id) {
            var model = _userLogic.Get(id);
            var vm = new UserManagerViewModel() {
                Id = model.Id,
                UserName = model.UserName
            };

            return View(vm);
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection) {
            try {
                return RedirectToAction(nameof(Index));
            } catch {
                return View();
            }
        }

        [HttpGet]
        [Route("/admin/usermanager/edit/{id}")]
        public ActionResult Edit(string id) {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection) {
            try {
                return RedirectToAction(nameof(Index));
            } catch {
                return View();
            }
        }

        [HttpGet]
        [Route("/admin/usermanager/delete/{id}")]
        public ActionResult Delete(string id) {
            var model = _userLogic.Get(id);
            var vm = new UserManagerViewModel() {
                Id = model.Id,
                UserName = model.UserName
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, IFormCollection collection) {
            try {
                return RedirectToAction(nameof(Index));
            } catch {
                return View();
            }
        }

        public ActionResult Search(string searchString = "") {
            var usersViewModel = new List<UserManagerViewModel>();

            if (searchString == "") {
                RedirectToAction("Index", "Home");
            }

            // Search Records
            var users = _userLogic.GetAll().Where(x => x.UserName.Contains(searchString.Trim())).ToList(); 

            return View(users);
        }
    }
}
