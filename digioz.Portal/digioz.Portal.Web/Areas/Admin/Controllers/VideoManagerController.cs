using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Bo;
using digioz.Portal.Dal;
using Microsoft.AspNetCore.Authorization;
using digioz.Portal.Bll.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using digioz.Portal.Web.Helpers;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class VideoManagerController : Controller
    {
        private readonly ILogic<Video> _videoLogic;
        private readonly ILogic<VideoAlbum> _videoAlbumLogic;
        private readonly IConfigLogic _configLogic;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VideoManagerController(
            ILogic<Video> videoLogic,
            ILogic<VideoAlbum> videoAlbumLogic,
            IConfigLogic configLogic,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _videoLogic = videoLogic;
            _videoAlbumLogic = videoAlbumLogic;
            _configLogic = configLogic;
            _userManager = userManager;
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

        // GET: Admin/VideoManager
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> VideoList(int albumId)
        {
            var videos = _videoLogic.GetAll(); // db.Videos.Include(p => p.AspNetUser).Include(p => p.VideoAlbum);
            
            if (albumId > 0)
            {
                videos = videos.Where(p => p.AlbumId == albumId).ToList();
            }

            List<SelectListItem> albums = new List<SelectListItem>();

            foreach (var album in _videoAlbumLogic.GetAll())
            {
                albums.Add(new SelectListItem { Text = album.Name, Value = album.Id.ToString() });
            }

            ViewBag.AlbumId = albums;

            return View(videos);
        }

        // GET: Admin/VideoManager/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = _videoLogic.Get(id);

            if (video != null)
            {
                var user = await _userManager.FindByIdAsync(video.UserId);

                if (user != null)
                {
                    ViewBag.User = user.Email;
                }

                var album = _videoAlbumLogic.Get(id);

                if (album != null)
                {
                    ViewBag.Album = album.Name;
                }
            }

            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        // GET: Admin/VideoManager/Create
        public async Task<IActionResult> Create()
        {
            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName");
            ViewBag.AlbumId = new SelectList(_videoAlbumLogic.GetAll(), "Id", "Name");

            return View();
        }

        // POST: Admin/VideoManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePosted([Bind("Id,UserId,AlbumId,Filename,Description,Approved,Visible,Thumbnail,Timestamp")] Video video, IFormFile file, IFormFile fileVideo)
        {
            if (file == null)
            {
                return RedirectToAction("Create", video);
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

                    video.Filename = fileNameVideo;
                    video.Thumbnail = fileName;

                    // Save Original Video
                    if (fileVideo != null && Utility.IsFileAVideo(fileVideo.FileName))
                    {
                        using (Stream fileStream = new FileStream(pathFull, FileMode.Create))
                        {
                            await fileVideo.CopyToAsync(fileStream);
                        }
                    }
                }

                video.Timestamp = DateTime.Now;
                _videoLogic.Add(video);

                return RedirectToAction("VideoList");
            }

            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", video.UserId);
            ViewBag.AlbumId = new SelectList(_videoAlbumLogic.GetAll(), "Id", "Name", video.AlbumId);

            return View(video);
        }

        // GET: Admin/VideoManager/Edit/5
        [Route("/admin/videomanager/edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = _videoLogic.Get(id);

            if (video == null)
            {
                return NotFound();
            }

            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", video.UserId);
            ViewBag.AlbumId = new SelectList(_videoAlbumLogic.GetAll(), "Id", "Name", video.AlbumId);

            return View(video);
        }

        // POST: Admin/VideoManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPosted(int id, [Bind("Id,UserId,AlbumId,Filename,Description,Approved,Visible,Thumbnail,Timestamp")] Video video, IFormFile file, IFormFile fileVideo)
        {
            if (id != video.Id)
            {
                return NotFound();
            }

            var videoExisting = _videoLogic.Get(video.Id);
            var album = _videoAlbumLogic.Get(video.AlbumId); 
            videoExisting.UserId = video.UserId;
            videoExisting.AlbumId = album.Id; 
            videoExisting.Description = video.Description;
            videoExisting.Timestamp = DateTime.Now;
            videoExisting.Approved = video.Approved;
            videoExisting.Visible = video.Visible;

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

                    video.Filename = fileNameVideo;
                    video.Thumbnail = fileName;

                    // Save Original Video
                    if (fileVideo != null && Utility.IsFileAVideo(fileVideo.FileName))
                    {
                        using (Stream fileStream = new FileStream(pathFull, FileMode.Create))
                        {
                            await fileVideo.CopyToAsync(fileStream);
                        }
                    }
                }

                videoExisting.Timestamp = DateTime.Now;
                _videoLogic.Edit(videoExisting);

                return RedirectToAction("VideoList");
            }

            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", video.UserId);
            ViewBag.AlbumId = new SelectList(_videoAlbumLogic.GetAll(), "Id", "Name", video.AlbumId);

            return View(video);
        }

        // GET: Admin/VideoManager/Delete/5
        [Route("/admin/videomanager/delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var video = _videoLogic.Get(id);

            if (video != null)
            {
                var user = await _userManager.FindByIdAsync(video.UserId);

                if (user != null)
                {
                    ViewBag.User = user.Email;
                }

                var album = _videoAlbumLogic.Get(id);

                if (album != null)
                {
                    ViewBag.Album = album.Name;
                }
            }

            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }

        // POST: Admin/VideoManager/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var video = _videoLogic.Get(id);

            _videoLogic.Delete(video);

            return RedirectToAction("VideoList");
        }

        [Route("/admin/videomanager/approve")]
        public ActionResult Approve()
        {
            var videos = _videoLogic.GetAll().Where(x => x.Approved == false || x.Visible == false).ToList();

            return View(videos.ToList());
        }

        [Route("/admin/videomanager/approvevideos/{id}")]
        public async Task<IActionResult> ApproveVideos(int id)
        {
            Video video = _videoLogic.Get(id);

            if (video == null)
            {
                return NotFound();
            }

            video.Approved = true;
            video.Visible = true;

            _videoLogic.Edit(video);

            return RedirectToAction("Approve");
        }

        private bool VideoExists(int id)
        {
            var videoExists = false; 
            var video = _videoLogic.Get(id); 

            if (video != null)
            {
                videoExists = true; 
            }

            return videoExists;
        }
    }
}
