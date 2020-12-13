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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public UserManagerController(
            ILogic<AspNetUser> userLogic,
            ILogic<Profile> profileLogic,
            UserManager<IdentityUser> usrManager,
            IPasswordHasher<IdentityUser> passwordHasher,
            IWebHostEnvironment webHostEnvironment,
            ILogic<AspNetRole> roleLogic,
            ILogic<AspNetUserRole> userRolesLogic
        ) {
            _userLogic = userLogic;
            _profileLogic = profileLogic;
            _userManager = usrManager;
            _passwordHasher = passwordHasher;
            _webHostEnvironment = webHostEnvironment;
            _roleLogic = roleLogic;
            _userRolesLogic = userRolesLogic;
        }

        private string GetImageFolderPath() {
            var webRootPath = _webHostEnvironment.WebRootPath;
            //var contentRootPath = _webHostEnvironment.ContentRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        private static async Task CropImageAndSave(UserManagerViewModel userVM, string path, int width, int height) {
            using var memoryStream = new MemoryStream();
            await userVM.AvatarImage.CopyToAsync(memoryStream);
            using var img = Image.FromStream(memoryStream);
            Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
        }

        public ActionResult Index() {
            var users = _userLogic.GetAll();

            return View(users);
        }

        [HttpGet]
        [Route("/admin/usermanager/details/{id}")]
        public ActionResult Details(string id) {
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

        public IActionResult Create() {
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
        public IActionResult Edit(string id) {
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
        public async Task<IActionResult> EditPost([Bind("Id", "UserID", "Email", "Birthday", "BirthdayVisible", "Address", "Address2", "City",
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
        public ActionResult Delete(string id) {
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
        public ActionResult DeletePost([Bind("Id")] UserManagerViewModel userVM) {
            var user = _userLogic.Get(userVM.Id);
            var profile = _profileLogic.GetAll().Where(x => x.UserId == user.Id).SingleOrDefault();

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

        [HttpGet]
        [Route("/admin/usermanager/roles/{id}")]
        public async Task<ActionResult> RolesAsync(string id) {
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

            return View(currentUserRoles);
        }

        [HttpGet]
        [Route("/admin/usermanager/roledelete/{id}/{userId}")]
        public async Task<ActionResult> RoleDeleteAsync(string id, string userId) {
            var user = await _userManager.FindByIdAsync(userId);
            var role = _roleLogic.Get(id);
            var userRole = _userRolesLogic.GetAll().Where(item => item.RoleId == id && item.UserId == userId).FirstOrDefault();
            _userRolesLogic.Delete(userRole);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("/admin/usermanager/roleadd/{id}/{userId}")]
        public ActionResult RoleAdd(string id, string userId) {
            // ToDo - Add Role to User

            return RedirectToAction("Index");
        }
    }
}
