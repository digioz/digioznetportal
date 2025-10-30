using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
 public class VideoAlbumDeleteModel : PageModel
 {
 private readonly IVideoAlbumService _albumService;
 private readonly IVideoService _videoService;
 private readonly IWebHostEnvironment _env;
 public VideoAlbumDeleteModel(IVideoAlbumService albumService, IVideoService videoService, IWebHostEnvironment env)
 { _albumService = albumService; _videoService = videoService; _env = env; }
 [BindProperty(SupportsGet = true)] public int Id { get; set; }
 public digioz.Portal.Bo.VideoAlbum? Item { get; private set; }
 public IActionResult OnGet(int id)
 {
 Item = _albumService.Get(id);
 if (Item == null) return RedirectToPage("/Video/VideoAlbumIndex", new { area = "Admin" });
 return Page();
 }
 public IActionResult OnPost()
 {
 var vids = _videoService.GetAll().Where(v => v.AlbumId == Id).ToList();
 foreach (var v in vids)
 {
 TryDeleteExistingFiles(v);
 _videoService.Delete(v.Id);
 }
 _albumService.Delete(Id);
 return RedirectToPage("/Video/VideoAlbumIndex", new { area = "Admin" });
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
