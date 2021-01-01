using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using digioz.Portal.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class UserManagerController : Controller
    {
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Profile> _profileLogic;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogic<AspNetRole> _roleLogic;
        private readonly ILogic<AspNetUserRole> _userRolesLogic;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagerController(
            ILogic<AspNetUser> userLogic,
            ILogic<Profile> profileLogic,
            UserManager<IdentityUser> usrManager,
            IPasswordHasher<IdentityUser> passwordHasher,
            IWebHostEnvironment webHostEnvironment,
            ILogic<AspNetRole> roleLogic,
            ILogic<AspNetUserRole> userRolesLogic,
            RoleManager<IdentityRole> roleManager
        ) {
            _userLogic = userLogic;
            _profileLogic = profileLogic;
            _userManager = usrManager;
            _passwordHasher = passwordHasher;
            _webHostEnvironment = webHostEnvironment;
            _roleLogic = roleLogic;
            _userRolesLogic = userRolesLogic;
            _roleManager = roleManager;
        }

        private string GetImageFolderPath() {
            var webRootPath = _webHostEnvironment.WebRootPath;
            //var contentRootPath = _webHostEnvironment.ContentRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        private async Task CropImageAndSave(UserManagerViewModel userVM, string path, int width, int height) {
            using var memoryStream = new MemoryStream();
            await userVM.AvatarImage.CopyToAsync(memoryStream);
            using var img = Image.FromStream(memoryStream);
            Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
        }

        public async Task<IActionResult> Index() {
            var users = _userLogic.GetAll();

            return View(users);
        }

        [HttpGet]
        [Route("/admin/usermanager/details/{id}")]
        public async Task<IActionResult> Details(string id) {
            var user = _userLogic.Get(id);
            var profile = _profileLogic.GetAll().Where(x => x.UserId == user.Id).SingleOrDefault();

            var vm = new UserManagerViewModel() {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            if (profile != null) {
                var excludes = new List<string>(new string[] { "Id" });
                ValueInjecter.CopyPropertiesTo(profile, vm, excludes);
                vm.Id = user.Id;
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
                } else {
                    var user = new IdentityUser {
                        UserName = userVM.Email,
                        Email = userVM.Email
                    };

                    var result = await _userManager.CreateAsync(user, userVM.Password);
                    var profileAvatarNew = string.Empty;

                    if (result.Succeeded) {
                        // Avatar Image Upload
                        if (userVM.AvatarImage != null && userVM.AvatarImage.Length > 0 && Helpers.Utility.IsImage(userVM.AvatarImage)) {
                            var imgFolder = GetImageFolderPath();
                            var lsFileName = Guid.NewGuid() + Path.GetExtension(userVM.AvatarImage.FileName);
                            var pathFull = Path.Combine(imgFolder, "Avatar", "Full", lsFileName);
                            var pathThumb = Path.Combine(imgFolder, "Avatar", "Thumb", lsFileName);

                            // Save Images
                            await CropImageAndSave(userVM, pathFull, 200, 200);
                            await CropImageAndSave(userVM, pathThumb, 100, 100);

                            profileAvatarNew = lsFileName;
                        }

                        var profileNew = new Profile();
                        var excludes = new List<string>(new string[] { "Id" });
                        ValueInjecter.CopyPropertiesTo(userVM, profileNew, excludes);
                        profileNew.UserId = user.Id;
                        profileNew.Avatar = profileAvatarNew;

                        _profileLogic.Add(profileNew);

                        return RedirectToAction("Index");
                    } else {
                        foreach (var error in result.Errors) {
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
            var profile = _profileLogic.GetAll().Where(x => x.UserId == user.Id).SingleOrDefault();

            var vm = new UserManagerViewModel() {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            if (profile != null) {
                var excludes = new List<string>(new string[] { "Id" });
                ValueInjecter.CopyPropertiesTo(profile, vm, excludes);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost([Bind("Id", "UserId", "Email", "Birthday", "BirthdayVisible", "Address", "Address2", "City",
                                                    "State", "Zip", "Country", "Signature", "Avatar", "FirstName", "LastName", "AvatarImage")] UserManagerViewModel userVM) {
            var user = await _userManager.FindByIdAsync(userVM.Id);

            if (user != null) {
                if (!string.IsNullOrEmpty(userVM.Email)) {
                    user.Email = userVM.Email;
                    user.UserName = userVM.Email;
                } else {
                    ModelState.AddModelError("", "Email cannot be empty.");
                }

                if (!string.IsNullOrEmpty(userVM.Password)) {
                    user.PasswordHash = _passwordHasher.HashPassword(user, userVM.Password);
                }

                if (!(userVM.Password == userVM.PasswordConfirm)) {
                    ModelState.AddModelError("", "Password confirmation does not match.");
                }

                if (!string.IsNullOrEmpty(userVM.Email)) {
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded) {
                        var profile = _profileLogic.GetAll().Where(x => x.UserId == user.Id).SingleOrDefault();
                        var profileAvatarNew = string.Empty;

                        // Avatar Image Upload
                        if (userVM.AvatarImage != null && userVM.AvatarImage.Length > 0 && Helpers.Utility.IsImage(userVM.AvatarImage)) {
                            var imgFolder = GetImageFolderPath();
                            var lsFileName = Guid.NewGuid() + Path.GetExtension(userVM.AvatarImage.FileName);
                            var pathFull = Path.Combine(imgFolder, "Avatar", "Full", lsFileName);
                            var pathThumb = Path.Combine(imgFolder, "Avatar", "Thumb", lsFileName);

                            // Save Images
                            await CropImageAndSave(userVM, pathFull, 200, 200);
                            await CropImageAndSave(userVM, pathThumb, 100, 100);

                            profileAvatarNew = lsFileName;
                        }

                        if (profile == null) {
                            var profileNew = new Profile();
                            var excludes = new List<string>(new string[] { "Id" });
                            ValueInjecter.CopyPropertiesTo(userVM, profileNew, excludes);
                            profileNew.UserId = user.Id;
                            profileNew.Avatar = profileAvatarNew;

                            _profileLogic.Add(profileNew);
                        } else {
                            var excludes = new List<string>(new string[] { "Id" });
                            ValueInjecter.CopyPropertiesTo(userVM, profile, excludes);
                            profile.UserId = user.Id;
                            profile.Avatar = profileAvatarNew;

                            _profileLogic.Edit(profile);
                        }

                        return RedirectToAction("Index");
                    }
                }
            } else {
                ModelState.AddModelError("", "User Not Found");
            }

            return View(user);
        }

        [HttpGet]
        [Route("/admin/usermanager/delete/{id}")]
        public async Task<IActionResult> Delete(string id) {
            var user = _userLogic.Get(id);
            var profile = _profileLogic.GetAll().Where(x => x.UserId == user.Id).SingleOrDefault();

            var vm = new UserManagerViewModel() {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            if (profile != null) {
                var excludes = new List<string>(new string[] { "Id" });
                ValueInjecter.CopyPropertiesTo(profile, vm, excludes);
                vm.Id = user.Id;
            }

            return View(vm);
        }

        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePostAsync([Bind("Id")] UserManagerViewModel userVM) {
            var user = await _userManager.FindByIdAsync(userVM.Id);
            var profile = _profileLogic.GetAll().Where(x => x.UserId == user.Id).SingleOrDefault();

            if (user != null)
            {
                if (profile != null) {
                    _profileLogic.Delete(profile);
                }

                // Remove user from all roles
                var userRoles = await _userManager.GetRolesAsync(user);

                foreach (var userRole in userRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, userRole);
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Search(string searchString = "") {
            var usersViewModel = new List<UserManagerViewModel>();

            if (searchString.IsNullEmpty()) {
                return RedirectToAction("Index", "UserManager");
            }

            // Search Records
            var users = _userLogic.GetAll().Where(x => x.UserName.Contains(searchString.Trim())).ToList();

            return View(users);
        }

        [HttpGet]
        [Route("/admin/usermanager/roles/{id}")]
        public async Task<IActionResult> RolesAsync(string id) {
            var user = await _userManager.FindByIdAsync(id);
            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = _roleLogic.GetAll();
            var currentUserRoles = new List<UserRoleViewModel>(); 

            foreach (var item in userRoles) {
                var currentUserRole = new UserRoleViewModel() {
                    UserId = id,
                    RoleId = roles.Where(x => x.Name == item).SingleOrDefault().Id,
                    RoleName = item
                };
                currentUserRoles.Add(currentUserRole);
            }

            ViewBag.UserId = id;
            ViewBag.Email = user.Email;

            return View(currentUserRoles);
        }

        [HttpGet]
        [Route("/admin/usermanager/roledelete/{id}/{userId}")]
        public async Task<IActionResult> RoleDeleteAsync(string id, string userId) {
            var user = await _userManager.FindByIdAsync(userId);
            var role = _roleLogic.Get(id);
            var userRole = _userRolesLogic.GetAll().Where(item => item.RoleId == id && item.UserId == userId).FirstOrDefault();
            _userRolesLogic.Delete(userRole);

            return RedirectToAction("Roles", "UserManager", new { id = userId });
        }

        [HttpGet]
        [Route("/admin/usermanager/roleadd/{id}")]
        public async Task<IActionResult> RoleAdd(string id) {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            ViewBag.UserId = id;

            return View(new IdentityRole());
        }

        [HttpPost, ActionName("RoleAddPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoleAddPostAsync([Bind("Id", "Name")] IdentityRole model, IFormCollection form)
        {
            string userId = form["UserId"];
            string roleId = form["Roles"];
            var user = await _userManager.FindByIdAsync(userId);
            var role = _roleManager.Roles.FirstOrDefault(x => x.Id == roleId);

            if (user != null && role != null)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }

            return RedirectToAction("Roles", "UserManager", new { id = userId });
        }
    }
}
