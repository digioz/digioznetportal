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
using Microsoft.AspNetCore.Http;
using digioz.Portal.Web.Helpers;
using System.IO;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class SlideShowManagerController : Controller
    {
        private readonly ILogic<SlideShow> _slideShowLogic;
        private readonly IConfigLogic _configLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SlideShowManagerController(
            ILogic<SlideShow> slideShowLogic,
            IConfigLogic configLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _slideShowLogic = slideShowLogic;
            _configLogic = configLogic;
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

        // GET: Admin/SlideShowManager
        public async Task<IActionResult> Index()
        {
            var slides = _slideShowLogic.GetAll();
            return View(slides);
        }

        // GET: Admin/SlideShowManager/Details/5

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slideShow = _slideShowLogic.Get(id);

            if (slideShow == null)
            {
                return NotFound();
            }

            return View(slideShow);
        }

        // GET: Admin/SlideShowManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/SlideShowManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Image,Description")] SlideShow slideShow, IFormFile file)
        {
            slideShow.Id = Guid.NewGuid().ToString();
             
            if (ModelState.IsValid)
            {
                // Upload Image
                if (file != null && Utility.IsFileAnImage(file.FileName))
                {
                    var imgFolder = GetImageFolderPath();
                    Guid guidName = Guid.NewGuid();
                    var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                    var pathFull = Path.Combine(imgFolder, "Slides", "Full", fileName);
                    var pathThumb = Path.Combine(imgFolder, "Slides", "Thumb", fileName);

                    // Save Original Image
                    await CropImageAndSave(file, pathFull, 850, 450);

                    // Save Thumbnail Image
                    await CropImageAndSave(file, pathThumb, 120, 120);

                    slideShow.Image = fileName;
                    slideShow.DateCreated = DateTime.Now;
                    slideShow.DateModified = DateTime.Now;
                }

                // Save Record
                _slideShowLogic.Add(slideShow);

                return RedirectToAction("Index");
            }

            return View(slideShow);
        }

        // GET: Admin/SlideShowManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SlideShow slideShow = _slideShowLogic.Get(id);

            if (slideShow == null)
            {
                return NotFound();
            }

            return View(slideShow);
        }

        // POST: Admin/SlideShowManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Image,Description")] SlideShow slideShow, IFormFile file)
        {
            var slideExisting = _slideShowLogic.Get(slideShow.Id);

            if (ModelState.IsValid)
            {
                // Upload Picture
                if (file != null && Utility.IsFileAnImage(file.FileName))
                {
                    var imgFolder = GetImageFolderPath();
                    Guid guidName = Guid.NewGuid();
                    var fileName = guidName.ToString() + Path.GetExtension(file.FileName);
                    var pathFull = Path.Combine(imgFolder, "Slides", "Full", fileName);
                    var pathThumb = Path.Combine(imgFolder, "Slides", "Thumb", fileName);

                    // Save Original Image
                    await CropImageAndSave(file, pathFull, 850, 450);

                    // Save Thumbnail Image
                    await CropImageAndSave(file, pathThumb, 120, 120);

                    slideShow.Image = fileName;
                    slideShow.DateModified = DateTime.Now;

                    slideExisting.Image = slideShow.Image;
                }

                // Save Record
                slideExisting.Description = slideShow.Description;
                _slideShowLogic.Edit(slideExisting);

                return RedirectToAction("Index");
            }

            return View(slideShow);
        }

        // GET: Admin/SlideShowManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SlideShow slideShow = _slideShowLogic.Get(id);

            if (slideShow == null)
            {
                return NotFound();
            }

            return View(slideShow);
        }

        // POST: Admin/SlideShowManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            SlideShow slideShow = _slideShowLogic.Get(id);

            if (slideShow != null)
            {
                _slideShowLogic.Delete(slideShow);
            }

            return RedirectToAction("Index");
        }
    }
}
