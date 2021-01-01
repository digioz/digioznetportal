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
    public class PictureManagerController : Controller
    {
        private readonly ILogic<Picture> _pictureLogic;
        private readonly ILogic<PictureAlbum> _pictureAlbumLogic;
        private readonly IConfigLogic _configLogic;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PictureManagerController(
            ILogic<Picture> pictureLogic,
            ILogic<PictureAlbum> pictureAlbumLogic,
            IConfigLogic configLogic,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _pictureLogic = pictureLogic;
            _pictureAlbumLogic = pictureAlbumLogic;
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

        // GET: Admin/PictureManager
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> PictureList(int albumId)
        {
            var pictures = _pictureLogic.GetAll(); // db.Pictures.Include(p => p.AspNetUser).Include(p => p.PictureAlbum);
            
            if (albumId > 0)
            {
                pictures = pictures.Where(p => p.AlbumId == albumId).ToList();
            }

            List<SelectListItem> albums = new List<SelectListItem>();

            foreach (var album in _pictureAlbumLogic.GetAll())
            {
                albums.Add(new SelectListItem { Text = album.Name, Value = album.Id.ToString() });
            }

            ViewBag.AlbumId = albums;

            return View(pictures);
        }

        // GET: Admin/PictureManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var picture = _pictureLogic.Get(id);

            if (picture != null)
            {
                var user = await _userManager.FindByIdAsync(picture.UserId);

                if (user != null)
                {
                    ViewBag.User = user.Email;
                }

                var album = _pictureAlbumLogic.Get(id);

                if (album != null)
                {
                    ViewBag.Album = album.Name;
                }
            }

            if (picture == null)
            {
                return NotFound();
            }

            return View(picture);
        }

        // GET: Admin/PictureManager/Create
        public async Task<IActionResult> Create()
        {
            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName");
            ViewBag.AlbumId = new SelectList(_pictureAlbumLogic.GetAll(), "Id", "Name");

            return View();
        }

        // POST: Admin/PictureManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePosted([Bind("Id,UserId,AlbumId,Filename,Description,Approved,Visible,Thumbnail,Timestamp")] Picture picture, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return RedirectToAction("Create", picture);
            }

            if (ModelState.IsValid)
            {
                foreach (var file in files)
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

                        picture.Filename = fileName;
                        picture.Thumbnail = fileName;
                    }

                    picture.Timestamp = DateTime.Now;
                    _pictureLogic.Add(picture);
                }

                return RedirectToAction("PictureList");
            }

            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", picture.UserId);
            ViewBag.AlbumId = new SelectList(_pictureAlbumLogic.GetAll(), "Id", "Name", picture.AlbumId);

            return View(picture);
        }

        // GET: Admin/PictureManager/Edit/5
        [Route("/admin/picturemanager/edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var picture = _pictureLogic.Get(id);

            if (picture == null)
            {
                return NotFound();
            }

            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", picture.UserId);
            ViewBag.AlbumId = new SelectList(_pictureAlbumLogic.GetAll(), "Id", "Name", picture.AlbumId);

            return View(picture);
        }

        // POST: Admin/PictureManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPosted(int id, [Bind("Id,UserId,AlbumId,Filename,Description,Approved,Visible,Thumbnail,Timestamp")] Picture picture, IFormFile file)
        {
            if (id != picture.Id)
            {
                return NotFound();
            }

            var pictureExisting = _pictureLogic.Get(picture.Id);
            var album = _pictureAlbumLogic.Get(picture.AlbumId); 
            pictureExisting.UserId = picture.UserId;
            pictureExisting.AlbumId = album.Id; 
            pictureExisting.Description = picture.Description;
            pictureExisting.Timestamp = DateTime.Now;
            pictureExisting.Approved = picture.Approved;
            pictureExisting.Visible = picture.Visible;

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

                    pictureExisting.Filename = fileName;
                    pictureExisting.Thumbnail = fileName;
                }

                pictureExisting.Timestamp = DateTime.Now;
                _pictureLogic.Edit(pictureExisting);

                return RedirectToAction("PictureList");
            }

            var users = _userManager.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", picture.UserId);
            ViewBag.AlbumId = new SelectList(_pictureAlbumLogic.GetAll(), "Id", "Name", picture.AlbumId);

            return View(picture);
        }

        // GET: Admin/PictureManager/Delete/5
        [Route("/admin/picturemanager/delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var picture = _pictureLogic.Get(id);

            if (picture != null)
            {
                var user = await _userManager.FindByIdAsync(picture.UserId);

                if (user != null)
                {
                    ViewBag.User = user.Email;
                }

                var album = _pictureAlbumLogic.Get(id);

                if (album != null)
                {
                    ViewBag.Album = album.Name;
                }
            }

            if (picture == null)
            {
                return NotFound();
            }

            return View(picture);
        }

        // POST: Admin/PictureManager/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var picture = _pictureLogic.Get(id);

            _pictureLogic.Delete(picture);

            return RedirectToAction("PictureList");
        }

        [Route("/admin/picturemanager/approve")]
        public ActionResult Approve()
        {
            var pictures = _pictureLogic.GetAll().Where(x => x.Approved == false || x.Visible == false).ToList();

            return View(pictures.ToList());
        }

        [Route("/admin/picturemanager/approvepictures/{id}")]
        public async Task<IActionResult> ApprovePictures(int id)
        {
            Picture picture = _pictureLogic.Get(id);

            if (picture == null)
            {
                return NotFound();
            }

            picture.Approved = true;
            picture.Visible = true;

            _pictureLogic.Edit(picture);

            return RedirectToAction("Approve");
        }

        private bool PictureExists(int id)
        {
            var pictureExists = false; 
            var picture = _pictureLogic.Get(id); 

            if (picture != null)
            {
                pictureExists = true; 
            }

            return pictureExists;
        }
    }
}
