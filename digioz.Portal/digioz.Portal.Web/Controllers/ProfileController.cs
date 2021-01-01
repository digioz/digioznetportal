using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using digioz.Portal.Web.Models.ViewModels;
using System.IO;
using System.Drawing;
using digioz.Portal.Bll;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Bo;
using System.Security.Claims;
using digioz.Portal.Bll.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Profile> _profileLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogic<Picture> _pictureLogic;

        public ProfileController(
            ILogic<AspNetUser> userLogic,
            ILogic<Profile> profileLogic,
            IWebHostEnvironment webHostEnvironment,
            ILogic<Picture> pictureLogic
        ) {
            _userLogic = userLogic;
            _profileLogic = profileLogic;
            _webHostEnvironment = webHostEnvironment;
            _pictureLogic = pictureLogic;
        }

        private string GetImageFolderPath() {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        private async Task CropImageAndSave(UserManagerViewModel userVM, string path, int width, int height) {
            using var memoryStream = new MemoryStream();
            await userVM.AvatarImage.CopyToAsync(memoryStream);
            using var img = Image.FromStream(memoryStream);
            Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
        }

        [Authorize]
        [Route("/profile/index")]
        public async Task<IActionResult> Edit()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = _profileLogic.GetAll().Where(x => x.UserId == userID).SingleOrDefault();

            var user = _userLogic.Get(userID);

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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(UserManagerViewModel userVM)
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profileAvatarNew = string.Empty;

            if (userID == userVM.Id)
            {
                // Avatar Image Upload
                if (userVM.AvatarImage != null && userVM.AvatarImage.Length > 0 && Helpers.Utility.IsImage(userVM.AvatarImage)) {
                    // Save Images
                    var imgFolder = GetImageFolderPath();
                    var lsFileName = Guid.NewGuid() + Path.GetExtension(userVM.AvatarImage.FileName);
                    var pathFull = Path.Combine(imgFolder, "Avatar", "Full", lsFileName);
                    var pathThumb = Path.Combine(imgFolder, "Avatar", "Thumb", lsFileName);

                    await CropImageAndSave(userVM, pathFull, 200, 200);
                    await CropImageAndSave(userVM, pathThumb, 100, 100);

                    profileAvatarNew = lsFileName;
                }

                // Gravatar Image Upload
                if (userVM.UseGravatar)
                {
                    if (userVM.Email != null)
                    {
                        // Fetch Gravatar and upload it to folder
                        var gravatarHash = Helpers.Utility.HashEmailForGravatar(userVM.Email);
                        var gravatar = $"http://www.gravatar.com/avatar/{gravatarHash}";
                        var ext = Helpers.Utility.GetRemoteImageExtension(gravatar);

                        var lsFileName = gravatarHash + "." + ext;
                        var imgFolder = GetImageFolderPath();
                        var pathFull = Path.Combine(imgFolder, "Avatar", "Full", lsFileName);
                        var pathThumb = Path.Combine(imgFolder, "Avatar", "Thumb", lsFileName);

                        Helpers.Utility.SaveFileFromUrl(pathFull, gravatar + "?size=200");
                        Helpers.Utility.SaveFileFromUrl(pathThumb, gravatar + "?size=100");
                        profileAvatarNew = lsFileName;
                    }
                }

                var profile = _profileLogic.GetAll().Where(x => x.UserId == userVM.Id).SingleOrDefault();

                if (profile == null) {
                    var profileNew = new Profile();
                    var excludes = new List<string>(new string[] { "Id" });
                    ValueInjecter.CopyPropertiesTo(userVM, profileNew, excludes);
                    profileNew.UserId = userID;
                    profileNew.Avatar = profileAvatarNew;

                    _profileLogic.Add(profileNew);
                } else {
                    var excludes = new List<string>(new string[] { "Id" });
                    ValueInjecter.CopyPropertiesTo(userVM, profile, excludes);
                    profile.UserId = userID;
                    profile.Avatar = profileAvatarNew;

                    _profileLogic.Edit(profile);
                }
                    
                TempData["Saved"] = true;
                return RedirectToAction("Edit");
            }

            return View("Edit");
        }

        public async Task<IActionResult> ShowDetails(string userId)
        {
            var user = _userLogic.Get(userId);
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

        public async Task<FileResult> ShowAvatar(string userId)
        {
            var profile = _profileLogic.GetAll().Where(x => x.UserId == userId).SingleOrDefault();

            var imgFolder = GetImageFolderPath();
            var path = Path.Combine(imgFolder, "Avatar", "Full");

            var avatarUrl = Path.Combine(path, "Default.png");
            var mimeType = "image/png";

            if (profile != null && profile.Avatar != null)
            {
                mimeType = Helpers.Utility.GetMimeType(profile.Avatar);
                avatarUrl = Path.Combine(path, profile.Avatar);
            }

            var fileInfo = new FileInfo(avatarUrl);

            if (!fileInfo.Exists) {
                avatarUrl = Path.Combine(path, "Default.png");
            }     
            
            var image = new FileStreamResult(new FileStream(avatarUrl, FileMode.Open), mimeType);

            return image;
        }

        public async Task<IActionResult> Pictures(string userId, string userName)
		{
            AspNetUser user = null;
            List<Picture> pictures = null;

            if (userId != null) {
                user = _userLogic.Get(userId);

            } else if (userName != null) {
                user = _userLogic.GetAll().Where(x => x.UserName == userName).SingleOrDefault();
                userId = user.Id;
            } else {
                var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
                user = user = _userLogic.GetAll().Where(x => x.UserName == userName).SingleOrDefault();
            }

            if (user != null) {
                pictures = _pictureLogic.GetAll().Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).ToList();
                ViewBag.Username = user.UserName;
            }

            return View(pictures);
		}

        public async Task<IActionResult> PictureDelete(long id)
		{
			if (User.Identity.IsAuthenticated)
			{
               // Check to make sure user owns picture
               var picture = _pictureLogic.Get(id);
                var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _userLogic.GetAll().Where(x => x.UserName == username).SingleOrDefault();

                if (user != null && picture != null && user.Id == picture.UserId) {
                    // Delete Picture
                    _pictureLogic.Delete(picture);
                }
            }

			// Redirect back to Pictures
			return RedirectToAction("Pictures");
		}
	}
}