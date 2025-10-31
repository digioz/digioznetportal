using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
    public class VideoAlbumEditModel : PageModel
    {
        private readonly IVideoAlbumService _albumService;
        public VideoAlbumEditModel(IVideoAlbumService albumService) { _albumService = albumService; }
        [BindProperty] public digioz.Portal.Bo.VideoAlbum Item { get; set; }
        public IActionResult OnGet(int id)
        {
            Item = _albumService.Get(id);
            if (Item == null) return RedirectToPage("/Video/VideoAlbumIndex", new { area = "Admin" });
            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            _albumService.Update(Item);
            return RedirectToPage("/Video/VideoAlbumIndex", new { area = "Admin" });
        }
    }
}
