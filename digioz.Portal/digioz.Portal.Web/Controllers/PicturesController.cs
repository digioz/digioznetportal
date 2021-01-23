using digioz.Portal.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using X.PagedList;
using digioz.Portal.Bo;
using digioz.Portal.Bll;
using digioz.Portal.Web.Helpers;
using System.IO;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using digioz.Portal.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace digioz.Portal.Web.Controllers
{
    public class PicturesController : Controller
    {
        private readonly ILogic<Picture> _pictureLogic;
        private readonly ILogic<PictureAlbum> _pictureAlbumLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PicturesController(
            ILogic<Picture> pictureLogic,
            ILogic<PictureAlbum> pictureAlbumLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _pictureLogic = pictureLogic;
            _pictureAlbumLogic = pictureAlbumLogic;
            _configLogic = configLogic;
            _userLogic = userLogic;
            _webHostEnvironment = webHostEnvironment;
        }
        private string GetImageFolderPath()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        private async Task CropImageAndSave(IFormFile file, string path, int width, int height)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            using var img = Image.FromStream(memoryStream);
            Helpers.ImageHelper.SaveImageWithCrop(img, width, height, path);
        }

        //
        // GET: /Pictures/
        public async Task<IActionResult> Index()
        {
            List<PicturesViewModel> pictureAlbumList = new List<PicturesViewModel>();
            PicturesViewModel pictureAlbum;

            var pictureAlbums = _pictureAlbumLogic.GetAll().Where(x => x.Visible == true).ToList();

            foreach (var item in pictureAlbums)
            {
                pictureAlbum = new PicturesViewModel();
                pictureAlbum.ID = item.Id;
                pictureAlbum.Name = item.Name;
                pictureAlbum.Description = item.Description;
                pictureAlbum.Timestamp = item.Timestamp;
                pictureAlbum.Visible = item.Visible;
                pictureAlbum.Thumbnail = _pictureLogic.GetAll().OrderByDescending(x => x.Id).Where(x => x.AlbumId == item.Id && x.Visible == true && x.Approved == true).Select(x => x.Filename).FirstOrDefault();

                if (pictureAlbum.Thumbnail == null || pictureAlbum.Thumbnail == string.Empty)
                {
                    pictureAlbum.Thumbnail = "PictureAlbumIcon.png";
                }

                pictureAlbumList.Add(pictureAlbum);
            }

            return View(pictureAlbumList);
        }

        public async Task<IActionResult> List(int id, int? pageNumber)
        {
            var pictures = _pictureLogic.GetAll().Where(x => x.AlbumId == id && x.Visible == true && x.Approved == true).OrderByDescending(x => x.Id).ToList();

            int pageSize = 10;
            int pageNumberNew = (pageNumber ?? 1);
            ViewBag.PageNumber = pageNumber; 

            return View(pictures.ToPagedList(pageNumberNew, pageSize));
        }

		[Authorize]
		public async Task<IActionResult> Add()
		{
			ViewBag.UserId = new SelectList(_userLogic.GetAll(), "Id", "UserName");
			ViewBag.AlbumId = new SelectList(_pictureAlbumLogic.GetAll(), "Id", "Name");

			return View();
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddAsync(Picture model, IFormFile file)
		{
			if (file == null)
			{
				return RedirectToAction("Create", model);
			}

			if (ModelState.IsValid)
			{
                if (file != null && Utility.IsFileAnImage(file.FileName))
                {
                    var imgFolder = GetImageFolderPath();
                    Guid guidName = Guid.NewGuid();
                    var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                    var pathFull = Path.Combine(imgFolder, "Pictures", "Full", fileName);
                    var pathThumb = Path.Combine(imgFolder, "Pictures", "Thumb", fileName);

                    // Save Original Image
                    using (Stream fileStream = new FileStream(pathFull, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Save Thumbnail Image
                    await CropImageAndSave(file, pathThumb, 120, 120);

                    model.Filename = fileName;
                    model.Thumbnail = fileName;
				    model.Timestamp = DateTime.Now;

				    var username = User.Identity.Name;
                    var user = _userLogic.GetAll().SingleOrDefault(x => x.UserName == username);

				    model.UserId = user.Id;
				    model.Approved = false;
				    model.Visible = false;

				    _pictureLogic.Add(model); 
                }
			}

			ViewBag.UserId = new SelectList(_userLogic.GetAll(), "Id", "UserName", model.UserId);
			ViewBag.AlbumId = new SelectList(_pictureAlbumLogic.GetAll(), "Id", "Name", model.AlbumId);

			return RedirectToAction("Pictures", "Profile");
		}
    }
}