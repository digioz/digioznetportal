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
    public class PictureAlbumManagerController : Controller
    {
        private readonly ILogic<PictureAlbum> _pictureAlbumLogic;
        private readonly IConfigLogic _configLogic;

        public PictureAlbumManagerController(
            ILogic<PictureAlbum> pictureAlbumLogic,
            IConfigLogic configLogic
        )
        {
            _pictureAlbumLogic = pictureAlbumLogic;
            _configLogic = configLogic;
        }

        // GET: Admin/PictureAlbumManager
        public async Task<IActionResult> Index()
        {
            return View(_pictureAlbumLogic.GetAll());
        }

        // GET: Admin/PictureAlbumManager/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pictureAlbum = _pictureAlbumLogic.Get(id);

            if (pictureAlbum == null)
            {
                return NotFound();
            }

            return View(pictureAlbum);
        }

        // GET: Admin/PictureAlbumManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/PictureAlbumManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Approved,Visible,Timestamp")] PictureAlbum pictureAlbum)
        {
            pictureAlbum.Timestamp = DateTime.Now;

            if (ModelState.IsValid)
            {
                _pictureAlbumLogic.Add(pictureAlbum); 

                return RedirectToAction(nameof(Index));
            }

            return View(pictureAlbum);
        }

        // GET: Admin/PictureAlbumManager/Edit/5
        [Route("/admin/picturealbummanager/edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pictureAlbum = _pictureAlbumLogic.Get(id);

            if (pictureAlbum == null)
            {
                return NotFound();
            }

            return View(pictureAlbum);
        }

        // POST: Admin/PictureAlbumManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPosted(int id, [Bind("Id,Name,Description,Approved,Visible,Timestamp")] PictureAlbum pictureAlbum)
        {
            pictureAlbum.Timestamp = DateTime.Now;

            if (id != pictureAlbum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _pictureAlbumLogic.Edit(pictureAlbum); 
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PictureAlbumExists(pictureAlbum.Id))
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

            return View(pictureAlbum);
        }

        // GET: Admin/PictureAlbumManager/Delete/5
        [Route("/admin/picturealbummanager/delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pictureAlbum = _pictureAlbumLogic.Get(id);

            if (pictureAlbum == null)
            {
                return NotFound();
            }

            return View(pictureAlbum);
        }

        // POST: Admin/PictureAlbumManager/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pictureAlbum = _pictureAlbumLogic.Get(id);
            _pictureAlbumLogic.Delete(pictureAlbum); 

            return RedirectToAction(nameof(Index));
        }

        private bool PictureAlbumExists(int id)
        {
            var exists = false;

            var pictureAlbum = _pictureAlbumLogic.Get(id); 

            if (pictureAlbum != null)
            {
                exists = true; 
            }

            return exists; 
        }
    }
}
