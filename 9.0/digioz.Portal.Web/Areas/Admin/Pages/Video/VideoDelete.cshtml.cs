using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
    public class VideoDeleteModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IWebHostEnvironment _env;
        public VideoDeleteModel(IVideoService videoService, IWebHostEnvironment env)
        { _videoService = videoService; _env = env; }
        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public digioz.Portal.Bo.Video? Item { get; private set; }
        public IActionResult OnGet(int id)
        {
            Item = _videoService.Get(id);
            if (Item == null) return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });
            return Page();
        }
        public IActionResult OnPost()
        {
            var item = _videoService.Get(Id);
            if (item != null)
            {
                TryDeleteExistingFiles(item);
                _videoService.Delete(Id);
            }
            return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });
        }
        private void TryDeleteExistingFiles(digioz.Portal.Bo.Video vid)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, "img", "Videos", "Full", vid.Filename ?? "");
                var thumbPath = Path.Combine(_env.WebRootPath, "img", "Videos", "Thumb", vid.Thumbnail ?? "");
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                if (System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath);
            }
            catch { }
        }
    }
}
