using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
 public class VideoAddModel : PageModel
 {
 private readonly IVideoService _videoService;
 private readonly IVideoAlbumService _albumService;
 private readonly IUserHelper _userHelper;
 private readonly IWebHostEnvironment _env;

 public VideoAddModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env)
 {
 _videoService = videoService;
 _albumService = albumService;
 _userHelper = userHelper;
 _env = env;
 }

 [BindProperty] public digioz.Portal.Bo.Video Item { get; set; } = new digioz.Portal.Bo.Video { Visible = true, Approved = false, Timestamp = DateTime.UtcNow };
 [BindProperty] public IFormFile? ThumbnailFile { get; set; }
 [BindProperty] public IFormFile? VideoFile { get; set; }
 public List<VideoAlbum> Albums { get; private set; } = new();

 public void OnGet()
 {
 Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
 if (!ModelState.IsValid) return Page();
 if (ThumbnailFile == null || ThumbnailFile.Length ==0)
 {
 ModelState.AddModelError("ThumbnailFile", "Please select a thumbnail image to upload.");
 return Page();
 }
 if (VideoFile == null || VideoFile.Length ==0)
 {
 ModelState.AddModelError("VideoFile", "Please select a video to upload.");
 return Page();
 }

 var thumbExt = Path.GetExtension(ThumbnailFile.FileName).ToLowerInvariant();
 var imgAllowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
 if (!imgAllowed.Contains(thumbExt))
 {
 ModelState.AddModelError("ThumbnailFile", "Invalid image type.");
 return Page();
 }
 var videoExt = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
 var vidAllowed = new[] { ".mp4", ".mov", ".avi", ".wmv", ".mkv", ".mpg", ".mpeg" };
 if (!vidAllowed.Contains(videoExt))
 {
 ModelState.AddModelError("VideoFile", "Invalid video type.");
 return Page();
 }

 var webroot = _env.WebRootPath;
 var fullDir = Path.Combine(webroot, "img", "Videos", "Full");
 var thumbDir = Path.Combine(webroot, "img", "Videos", "Thumb");
 Directory.CreateDirectory(fullDir);
 Directory.CreateDirectory(thumbDir);

 var guid = Guid.NewGuid().ToString("N");
 var videoName = guid + videoExt;
 var thumbName = guid + thumbExt;
 var fullPath = Path.Combine(fullDir, videoName);
 var thumbPath = Path.Combine(thumbDir, thumbName);

 // Save video
 using (var fs = System.IO.File.Create(fullPath))
 {
 await VideoFile.CopyToAsync(fs);
 }

 // Save thumbnail (crop150x150)
 using (var ms = new MemoryStream())
 {
 await ThumbnailFile.CopyToAsync(ms);
 ms.Position =0;
 using var image = System.Drawing.Image.FromStream(ms);
 ImageHelper.SaveImageWithCrop(image,150,150, thumbPath);
 }

 Item.Filename = videoName;
 Item.Thumbnail = thumbName;
 Item.Timestamp = DateTime.UtcNow;
 var email = User?.Identity?.Name;
 Item.UserId = _userHelper.GetUserIdByEmail(email);

 _videoService.Add(Item);
 return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });
 }
 }
}
