using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.Utilities;
using digiozPortal.Web.Areas.Admin.Models;
using digiozPortal.Web.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace digiozPortal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class UserManagerController : Controller
    {
        private readonly ILogic<AspNetUsers> _userLogic;
        private readonly ILogic<Profile> _profileLogic;
        private UserManager<IdentityUser> _userManager;
        private IPasswordHasher<IdentityUser> _passwordHasher;

        public UserManagerController(
            ILogic<AspNetUsers> userLogic,
            ILogic<Profile> profileLogic,
            UserManager<IdentityUser> usrManager,
            IPasswordHasher<IdentityUser> passwordHasher
        ) 
        {
            _userLogic = userLogic;
            _profileLogic = profileLogic;
            _userManager = usrManager;
            _passwordHasher = passwordHasher;
        }

        public ActionResult Index() {
            var users = _userLogic.GetAll();

            return View(users);
        }

        [HttpGet]
        [Route("/admin/usermanager/details/{id}")]
        public ActionResult Details(string id) {
            var user = _userLogic.Get(id);
            var profile = _profileLogic.GetAll().Where(x => x.UserID == user.Id).SingleOrDefault();

            var vm = new UserManagerViewModel() {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            if (profile != null) {
                vm.Birthday = profile.Birthday;
                vm.BirthdayVisible = profile.BirthdayVisible;
                vm.Address = profile.Address;
                vm.Address2 = profile.Address2;
                vm.City = profile.City;
                vm.State = profile.State;
                vm.Zip = profile.Zip;
                vm.Country = profile.Country;
                vm.Signature = profile.Signature;
                vm.Avatar = profile.Avatar;
                vm.FirstName = profile.FirstName;
                vm.LastName = profile.LastName;
            }

            return View(vm);
        }

        public async Task<IActionResult> Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserManagerViewModel userVM) {
            userVM.UserName = userVM.Email;

            if (!string.IsNullOrEmpty(userVM.Email)) {
                if (userVM.Password != userVM.PasswordConfirm) {
                    ModelState.AddModelError("", "Password confirmation does not match.");
                }
                else {
                    var appUser = new IdentityUser {
                        UserName = userVM.Email,
                        Email = userVM.Email
                    };

                    IdentityResult result = await _userManager.CreateAsync(appUser, userVM.Password);

                    if (result.Succeeded) {
                        return RedirectToAction("Index");
                    }
                    else {
                        foreach (IdentityError error in result.Errors) {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }

            }

            return View(userVM);
        }

        [HttpGet]
        [Route("/admin/usermanager/edit/{id}")]
        public async Task<IActionResult> Edit(string id) {
            var user = _userLogic.Get(id);
            var profile = _profileLogic.GetAll().Where(x => x.UserID == user.Id).SingleOrDefault();

            var vm = new UserManagerViewModel() {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            // ToDo - Use Value Injector
            if (profile != null) {
                vm.Email = profile.Email;
                vm.Birthday = profile.Birthday;
                vm.BirthdayVisible = profile.BirthdayVisible;
                vm.Address = profile.Address;
                vm.Address2 = profile.Address2;
                vm.City = profile.City;
                vm.State = profile.State;
                vm.Zip = profile.Zip;
                vm.Country = profile.Country;
                vm.Signature = profile.Signature;
                vm.Avatar = profile.Avatar;
                vm.FirstName = profile.FirstName;
                vm.LastName = profile.LastName;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost([Bind("Id", "UserID", "Email", "Birthday", "BirthdayVisible", "Address", "Address2", "City", 
                                                    "State", "Zip", "Country", "Signature", "Avatar", "FirstName", "LastName")] UserManagerViewModel userVM) {
            var user = await _userManager.FindByIdAsync(userVM.Id);

            // ToDo - Handle Avatar File Upload
            if (user != null) {
                if (!string.IsNullOrEmpty(userVM.Email)) {
                    user.Email = userVM.Email;
                    user.UserName = userVM.Email;
                }
                else {
                    ModelState.AddModelError("", "Email cannot be empty.");
                }

                if (!string.IsNullOrEmpty(userVM.Password)) {
                    user.PasswordHash = _passwordHasher.HashPassword(user, userVM.Password);
                }

                if (!(userVM.Password == userVM.PasswordConfirm)) {
                    ModelState.AddModelError("", "Password confirmation does not match.");
                }

                if (!string.IsNullOrEmpty(userVM.Email)) {
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded) {
                        var profile = _profileLogic.GetAll().Where(x => x.UserID == user.Id).SingleOrDefault();

                        if (profile == null) {
                            var profileNew = new Profile() {
                                UserID = userVM.Id,
                                Email = userVM.Email,
                                Birthday = userVM.Birthday,
                                BirthdayVisible = userVM.BirthdayVisible,
                                Address = userVM.Address,
                                Address2 = userVM.Address2,
                                City = userVM.City,
                                State = userVM.State,
                                Zip = userVM.Zip,
                                Country = userVM.Country,
                                Signature = userVM.Signature,
                                Avatar = userVM.Avatar,
                                FirstName = userVM.FirstName,
                                LastName = userVM.LastName
                            };

                            _profileLogic.Add(profileNew);
                        }
                        else {
                            profile.Email = profile.Email;
                            profile.Birthday = profile.Birthday;
                            profile.BirthdayVisible = profile.BirthdayVisible;
                            profile.Address = profile.Address;
                            profile.Address2 = profile.Address2;
                            profile.City = profile.City;
                            profile.State = profile.State;
                            profile.Zip = profile.Zip;
                            profile.Country = profile.Country;
                            profile.Signature = profile.Signature;
                            profile.Avatar = profile.Avatar;
                            profile.FirstName = profile.FirstName;
                            profile.LastName = profile.LastName;

                            _profileLogic.Edit(profile);
                        }

                        return RedirectToAction("Index");
                    }
                }
            } 
            else {
                ModelState.AddModelError("", "User Not Found");
            }

            return View(user);
        }

        [HttpGet]
        [Route("/admin/usermanager/delete/{id}")]
        public ActionResult Delete(string id) {
            var user = _userLogic.Get(id);
            var profile = _profileLogic.GetAll().Where(x => x.UserID == user.Id).SingleOrDefault();

            var vm = new UserManagerViewModel() {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            if (profile != null) {
                vm.Birthday = profile.Birthday;
                vm.BirthdayVisible = profile.BirthdayVisible;
                vm.Address = profile.Address;
                vm.Address2 = profile.Address2;
                vm.City = profile.City;
                vm.State = profile.State;
                vm.Zip = profile.Zip;
                vm.Country = profile.Country;
                vm.Signature = profile.Signature;
                vm.Avatar = profile.Avatar;
                vm.FirstName = profile.FirstName;
                vm.LastName = profile.LastName;
            }

            return View(vm);
        }

        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost([Bind("Id")] UserManagerViewModel userVM) {
            var user = _userLogic.Get(userVM.Id);
            var profile = _profileLogic.GetAll().Where(x => x.UserID == user.Id).SingleOrDefault();

            if (profile != null) {
                _profileLogic.Delete(profile);
            }

            _userLogic.Delete(user);

            return RedirectToAction("Index");
        }

        public ActionResult Search(string searchString = "") {
            var usersViewModel = new List<UserManagerViewModel>();

            if (searchString.IsNullEmpty()) {
                return RedirectToAction("Index", "UserManager");
            }

            // Search Records
            var users = _userLogic.GetAll().Where(x => x.UserName.Contains(searchString.Trim())).ToList(); 

            return View(users);
        }
    }
}
