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
    public class VideosController : Controller
    {
        private readonly ILogic<Video> _videoLogic;
        private readonly ILogic<VideoAlbum> _videoAlbumLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VideosController(
            ILogic<Video> videoLogic,
            ILogic<VideoAlbum> videoAlbumLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _videoLogic = videoLogic;
            _videoAlbumLogic = videoAlbumLogic;
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
        // GET: /Videos/
        public async Task<IActionResult> Index()
        {
            List<VideosViewModel> videoAlbumList = new List<VideosViewModel>();
            VideosViewModel videoAlbum;

            var videoAlbums = _videoAlbumLogic.GetAll().Where(x => x.Visible == true).ToList();

            foreach (var item in videoAlbums)
            {
                videoAlbum = new VideosViewModel();
                videoAlbum.ID = item.Id;
                videoAlbum.Name = item.Name;
                videoAlbum.Description = item.Description;
                videoAlbum.Timestamp = item.Timestamp;
                videoAlbum.Visible = item.Visible;
                videoAlbum.Thumbnail = _videoLogic.GetAll().OrderByDescending(x => x.Id).Where(x => x.AlbumId == item.Id && x.Visible == true && x.Approved == true).Select(x => x.Thumbnail).FirstOrDefault();

                if (videoAlbum.Thumbnail == null || videoAlbum.Thumbnail == string.Empty)
                {
                    videoAlbum.Thumbnail = "VideoAlbumIcon.png";
                }

                videoAlbumList.Add(videoAlbum);
            }

            return View(videoAlbumList);
        }

        public async Task<IActionResult> List(int id, int? pageNumber)
        {
            var videos = _videoLogic.GetAll().Where(x => x.AlbumId == id && x.Visible == true && x.Approved == true).OrderByDescending(x => x.Id).ToList();

            int pageSize = 10;
            int pageNumberNew = (pageNumber ?? 1);
            ViewBag.PageNumber = pageNumber; 

            return View(videos.ToPagedList(pageNumberNew, pageSize));
        }

		[Authorize]
		public async Task<IActionResult> Add()
		{
			ViewBag.AlbumId = new SelectList(_videoAlbumLogic.GetAll(), "Id", "Name");

			return View();
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddAsync(Video model, IFormFile file, IFormFile fileVideo)
		{
            ViewBag.AlbumId = new SelectList(_videoAlbumLogic.GetAll(), "Id", "Name", model.AlbumId);

            if (file == null)
			{
                ModelState.AddModelError(nameof(model.Filename), "You must select a file.");
                return View("Add", model);
            }

			if (ModelState.IsValid)
			{
                if (file != null && Utility.IsFileAnImage(file.FileName))
                {
                    var imgFolder = GetImageFolderPath();
                    Guid guidName = Guid.NewGuid();
                    var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                    var fileNameVideo = guidName.ToString() + Path.GetExtension(fileVideo.FileName);
                    var pathFull = Path.Combine(imgFolder, "Videos", "Full", fileNameVideo);
                    var pathThumb = Path.Combine(imgFolder, "Videos", "Thumb", fileName);

                    // Save Thumbnail Image
                    await CropImageAndSave(file, pathThumb, 240, 120);

                    // Save Original Video
                    if (fileVideo != null && Utility.IsFileAVideo(fileVideo.FileName))
                    {
                        using (Stream fileStream = new FileStream(pathFull, FileMode.Create))
                        {
                            await fileVideo.CopyToAsync(fileStream);
                        }
                    }

                    model.Filename = fileNameVideo;
                    model.Thumbnail = fileName;
				    model.Timestamp = DateTime.Now;

				    var username = User.Identity.Name;
                    var user = _userLogic.GetAll().SingleOrDefault(x => x.UserName == username);

				    model.UserId = user.Id;
				    model.Approved = false;
				    model.Visible = false;

				    _videoLogic.Add(model); 
                }

			    return RedirectToAction("Videos", "Profile");
			}

            return View("Add", model);
        }
    }
}