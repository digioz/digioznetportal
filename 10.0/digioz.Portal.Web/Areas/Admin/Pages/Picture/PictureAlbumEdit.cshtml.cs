using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureAlbumEditModel : PageModel
    {
        private readonly IPictureAlbumService _albumService;
        public PictureAlbumEditModel(IPictureAlbumService albumService)
        { _albumService = albumService; }
        [BindProperty] public PictureAlbum? Item { get; set; }
        public IActionResult OnGet(int id)
        {
            Item = _albumService.Get(id);
            if (Item == null) return RedirectToPage("/Picture/PictureAlbumIndex", new { area = "Admin" });
            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/Picture/PictureAlbumIndex", new { area = "Admin" });
            Item.Timestamp = DateTime.UtcNow;
            _albumService.Update(Item);
            return RedirectToPage("/Picture/PictureAlbumIndex", new { area = "Admin" });
        }
    }
}
