using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
 public class VideoEditModel : PageModel
 {
 private readonly IVideoService _videoService;
 private readonly IVideoAlbumService _albumService;
 private readonly digioz.Portal.Utilities.IUserHelper _userHelper;
 private readonly IWebHostEnvironment _env;

 public VideoEditModel(IVideoService videoService, IVideoAlbumService albumService, digioz.Portal.Utilities.IUserHelper userHelper, IWebHostEnvironment env)
 {
 _videoService = videoService;
 _albumService = albumService;
 _userHelper = userHelper;
 _env = env;
 }

 [BindProperty] public digioz.Portal.Bo.Video Item { get; set; }
 [BindProperty] public IFormFile? NewThumbnail { get; set; }
 [BindProperty] public IFormFile? NewVideo { get; set; }
 public List<digioz.Portal.Bo.VideoAlbum> Albums { get; private set; } = new();

 public IActionResult OnGet(int id)
 {
 Item = _videoService.Get(id);
 if (Item == null) return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });
 Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
 return Page();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
 if (!ModelState.IsValid) return Page();
 var existing = _videoService.Get(Item.Id);
 if (existing == null) return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });

 existing.AlbumId = Item.AlbumId;
 existing.Description = Item.Description;
 existing.Approved = Item.Approved;
 existing.Visible = Item.Visible;

 var webroot = _env.WebRootPath;
 var fullDir = Path.Combine(webroot, "img", "Videos", "Full");
 var thumbDir = Path.Combine(webroot, "img", "Videos", "Thumb");
 Directory.CreateDirectory(fullDir);
 Directory.CreateDirectory(thumbDir);

 if (NewThumbnail != null && NewThumbnail.Length > 0)
 {
 var ext = Path.GetExtension(NewThumbnail.FileName).ToLowerInvariant();
 var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
 if (!allowed.Contains(ext))
 {
 ModelState.AddModelError("NewThumbnail", "Invalid image type.");
 return Page();
 }
 var newThumb = Guid.NewGuid().ToString("N") + ext;
 var thumbPath = Path.Combine(thumbDir, newThumb);
 using (var ms = new MemoryStream())
 {
 await NewThumbnail.CopyToAsync(ms);
 ms.Position = 0;
 using var image = System.Drawing.Image.FromStream(ms);
 ImageHelper.SaveImageWithCrop(image,150,150, thumbPath);
 }
 TryDeleteFileIfExists(Path.Combine(thumbDir, existing.Thumbnail ?? string.Empty));
 existing.Thumbnail = newThumb;
 }

 if (NewVideo != null && NewVideo.Length > 0)
 {
 var ext = Path.GetExtension(NewVideo.FileName).ToLowerInvariant();
 var allowed = new[] { ".mp4", ".mov", ".avi", ".wmv", ".mkv", ".mpg", ".mpeg" };
 if (!allowed.Contains(ext))
 {
 ModelState.AddModelError("NewVideo", "Invalid video type.");
 return Page();
 }
 var newVid = Guid.NewGuid().ToString("N") + ext;
 var fullPath = Path.Combine(fullDir, newVid);
 using (var fs = System.IO.File.Create(fullPath))
 {
 await NewVideo.CopyToAsync(fs);
 }
 TryDeleteFileIfExists(Path.Combine(fullDir, existing.Filename ?? string.Empty));
 existing.Filename = newVid;
 }

 existing.Timestamp = DateTime.UtcNow;
 var email = User?.Identity?.Name;
 existing.UserId = _userHelper.GetUserIdByEmail(email);
 _videoService.Update(existing);
 return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });
 }

 private void TryDeleteFileIfExists(string path)
 {
 try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch { }
 }
 }
}
