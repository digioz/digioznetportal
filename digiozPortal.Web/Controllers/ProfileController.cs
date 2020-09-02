using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using digiozPortal.Web.Models.ViewModels;
using System.IO;
using System.Drawing;
using digiozPortal.BLL;
using digiozPortal.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using digiozPortal.BO;
using System.Security.Claims;
using digiozPortal.BLL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using digiozPortal.Web.Areas.Admin.Models.ViewModels;
using System.Threading.Tasks;

namespace digiozPortal.Web.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly ILogic<AspNetUsers> _userLogic;
        private readonly ILogic<Profile> _profileLogic;
        private UserManager<IdentityUser> _userManager;
        private IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(
            ILogic<AspNetUsers> userLogic,
            ILogic<Profile> profileLogic,
            UserManager<IdentityUser> usrManager,
            IPasswordHasher<IdentityUser> passwordHasher,
            IWebHostEnvironment webHostEnvironment
        ) {
            _userLogic = userLogic;
            _profileLogic = profileLogic;
            _userManager = usrManager;
            _passwordHasher = passwordHasher;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetImageFolderPath() {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string contentRootPath = _webHostEnvironment.ContentRootPath;

            string path = "";
            path = Path.Combine(webRootPath, "img");

            return path;
        }

        private async Task CropImageAndSave(UserManagerViewModel userVM, string path, int width, int height) {
            using (var memoryStream = new MemoryStream()) {
                await userVM.AvatarImage.CopyToAsync(memoryStream);
                using (var img = Image.FromStream(memoryStream)) {
                    Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
                }
            }
        }

        [Authorize]
        [Route("/profile/index")]
        public ActionResult Edit()
        {
            string userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = _profileLogic.GetAll().Where(x => x.UserID == userID).SingleOrDefault();

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
        public async Task<ActionResult> EditPost(UserManagerViewModel userVM)
        {
            string userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string profileAvatarNew = string.Empty;

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
                        string gravatarHash = Helpers.Utility.HashEmailForGravatar(userVM.Email);
                        string gravatar = $"http://www.gravatar.com/avatar/{gravatarHash}";
                        string ext = Helpers.Utility.GetRemoteImageExtension(gravatar);

                        string lsFileName = gravatarHash + "." + ext;
                        var imgFolder = GetImageFolderPath();
                        var pathFull = Path.Combine(imgFolder, "Avatar", "Full", lsFileName);
                        var pathThumb = Path.Combine(imgFolder, "Avatar", "Thumb", lsFileName);

                        Helpers.Utility.SaveFileFromUrl(pathFull, gravatar + "?size=200");
                        Helpers.Utility.SaveFileFromUrl(pathThumb, gravatar + "?size=100");
                        profileAvatarNew = lsFileName;
                    }
                }

                var profile = _profileLogic.GetAll().Where(x => x.UserID == userVM.Id).SingleOrDefault();

                if (profile == null) {
                    var profileNew = new Profile();
                    var excludes = new List<string>(new string[] { "Id" });
                    ValueInjecter.CopyPropertiesTo(userVM, profileNew, excludes);
                    profileNew.UserID = userID;
                    profileNew.Avatar = profileAvatarNew;

                    _profileLogic.Add(profileNew);
                } else {
                    var excludes = new List<string>(new string[] { "Id" });
                    ValueInjecter.CopyPropertiesTo(userVM, profile, excludes);
                    profile.UserID = userID;
                    profile.Avatar = profileAvatarNew;

                    _profileLogic.Edit(profile);
                }
                    
                _profileLogic.Edit(profile);

                TempData["Saved"] = true;
                return RedirectToAction("Edit");
            }

            return View("Edit");
        }

        public ActionResult ShowDetails(string userId, string userName)
        {
            var user = _userLogic.Get(userId);
            var profile = _profileLogic.GetAll().Where(x => x.UserID == user.Id).SingleOrDefault();

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

        public FileResult ShowAvatar(string userId)
        {
            var profile = _profileLogic.GetAll().Where(x => x.UserID == userId).SingleOrDefault();

            var imgFolder = GetImageFolderPath();
            var path = Path.Combine(imgFolder, "Avatar", "Full");

            string avatarUrl = Path.Combine(path, "Default.png");
            string mimeType = "image/png";

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

		public ActionResult Pictures(string userId, string userName)
		{
            //MembershipUser user = null;
            //List<Picture> pictures = null;

            //if (userId != null)
            //{
            //	user = AccountLogic.GetMembershipUser(userId);

            //}
            //else if (userName != null)
            //{
            //	user = AccountLogic.GetMembershipUserByUsername(userName);
            //	userId = user.Id;
            //}
            //else
            //{
            //	var username = User.Identity.GetUserName();
            //	user = AccountLogic.GetMembershipUserByUsername(username);
            //}

            //if (user != null)
            //{
            //	pictures = PictureLogic.GetAll().Where(x => x.UserID == user.Id).OrderByDescending(x => x.ID).ToList();
            //	ViewBag.Username = user.UserName;
            //}			

            //return View(pictures);

            return View();
		}

		public ActionResult PictureDelete(long id)
		{
			if (User.Identity.IsAuthenticated)
			{
				// Check to make sure user owns picture
				//var picture = PictureLogic.Get(id);
				//var username = User.Identity.GetUserName();
				//var user = AccountLogic.GetMembershipUserByUsername(username);

				//if (user != null && picture != null && user.Id == picture.UserID)
				//{
				//	// Delete Picture
				//	PictureLogic.Delete(picture.ID);
				//}
			}

			// Redirect back to Pictures
			return RedirectToAction("Pictures");
		}
	}
}