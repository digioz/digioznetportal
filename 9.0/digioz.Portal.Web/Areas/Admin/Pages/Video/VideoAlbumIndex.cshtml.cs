using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
 public class VideoAlbumIndexModel : PageModel
 {
 private readonly IVideoAlbumService _albumService;
 private readonly IVideoService _videoService;
 public VideoAlbumIndexModel(IVideoAlbumService albumService, IVideoService videoService)
 { _albumService = albumService; _videoService = videoService; }
 public IReadOnlyList<digioz.Portal.Bo.VideoAlbum> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.VideoAlbum>();
 [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } =1;
 [BindProperty(SupportsGet = true)] public int PageSize { get; set; } =10;
 public int TotalCount { get; private set; }
 public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
 public Dictionary<int, int> VideoCounts { get; private set; } = new();
 public Dictionary<int, string?> LatestThumbs { get; private set; } = new();
 public void OnGet()
 {
 var all = _albumService.GetAll().OrderByDescending(a => a.Id).ToList();
 TotalCount = all.Count;
 var skip = (PageNumber -1) * PageSize;
 Items = all.Skip(skip).Take(PageSize).ToList();
 var vids = _videoService.GetAll();
 VideoCounts = vids.GroupBy(v => v.AlbumId).ToDictionary(g => g.Key, g => g.Count());
 LatestThumbs = vids.GroupBy(v => v.AlbumId).ToDictionary(g => g.Key, g => g.OrderByDescending(v => v.Id).Select(v => v.Thumbnail).FirstOrDefault());
 }
 }
}
