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

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class VideoAlbumManagerController : Controller
    {
        private readonly ILogic<VideoAlbum> _videoAlbumLogic;
        private readonly IConfigLogic _configLogic;

        public VideoAlbumManagerController(
            ILogic<VideoAlbum> videoAlbumLogic,
            IConfigLogic configLogic
        )
        {
            _videoAlbumLogic = videoAlbumLogic;
            _configLogic = configLogic;
        }

        // GET: Admin/VideoAlbumManager
        public async Task<IActionResult> Index()
        {
            return View(_videoAlbumLogic.GetAll());
        }

        // GET: Admin/VideoAlbumManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var videoAlbum = _videoAlbumLogic.Get(id);

            if (videoAlbum == null)
            {
                return NotFound();
            }

            return View(videoAlbum);
        }

        // GET: Admin/VideoAlbumManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/VideoAlbumManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Approved,Visible,Timestamp")] VideoAlbum videoAlbum)
        {
            videoAlbum.Timestamp = DateTime.Now;

            if (ModelState.IsValid)
            {
                _videoAlbumLogic.Add(videoAlbum); 

                return RedirectToAction(nameof(Index));
            }

            return View(videoAlbum);
        }

        // GET: Admin/VideoAlbumManager/Edit/5
        [Route("/admin/videoalbummanager/edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var videoAlbum = _videoAlbumLogic.Get(id);

            if (videoAlbum == null)
            {
                return NotFound();
            }

            return View(videoAlbum);
        }

        // POST: Admin/VideoAlbumManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPosted(int id, [Bind("Id,Name,Description,Approved,Visible,Timestamp")] VideoAlbum videoAlbum)
        {
            videoAlbum.Timestamp = DateTime.Now;

            if (id != videoAlbum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _videoAlbumLogic.Edit(videoAlbum); 
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VideoAlbumExists(videoAlbum.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(videoAlbum);
        }

        // GET: Admin/VideoAlbumManager/Delete/5
        [Route("/admin/videoalbummanager/delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var videoAlbum = _videoAlbumLogic.Get(id);

            if (videoAlbum == null)
            {
                return NotFound();
            }

            return View(videoAlbum);
        }

        // POST: Admin/VideoAlbumManager/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var videoAlbum = _videoAlbumLogic.Get(id);
            _videoAlbumLogic.Delete(videoAlbum); 

            return RedirectToAction(nameof(Index));
        }

        private bool VideoAlbumExists(int id)
        {
            var exists = false;

            var videoAlbum = _videoAlbumLogic.Get(id); 

            if (videoAlbum != null)
            {
                exists = true; 
            }

            return exists; 
        }
    }
}
