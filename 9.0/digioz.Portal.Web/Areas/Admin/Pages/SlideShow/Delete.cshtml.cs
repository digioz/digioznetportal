using System.IO;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.SlideShow
{
    public class DeleteModel : PageModel
    {
        private readonly ISlideShowService _service;
        private readonly IWebHostEnvironment _env;

        public DeleteModel(ISlideShowService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public Bo.SlideShow? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/SlideShow/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            var item = _service.Get(Id);
            if (item != null)
            {
                TryDeleteExistingFiles(item);
                _service.Delete(Id);
            }
            return RedirectToPage("/SlideShow/Index", new { area = "Admin" });
        }

        private void TryDeleteExistingFiles(Bo.SlideShow slide)
        {
            try
            {
                if (!string.IsNullOrEmpty(slide.Image))
                {
                    var webroot = _env.WebRootPath;
                    var fullPath = Path.Combine(webroot, "img", "Slides", "Full", slide.Image);
                    var thumbPath = Path.Combine(webroot, "img", "Slides", "Thumb", slide.Image);
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                    if (System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath);
                }
            }
            catch { }
        }
    }
}
