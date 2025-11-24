using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.CommentConfig
{
    public class EditModel : PageModel
    {
        private readonly ICommentConfigService _configService;
        private readonly IPageService _pageService;
        private readonly IAnnouncementService _announcementService;
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;

        public EditModel(
            ICommentConfigService configService, 
            IPageService pageService, 
            IAnnouncementService announcementService,
            IPictureService pictureService,
            IVideoService videoService)
        {
            _configService = configService;
            _pageService = pageService;
            _announcementService = announcementService;
            _pictureService = pictureService;
            _videoService = videoService;
        }

        [BindProperty] public Bo.CommentConfig? Item { get; set; }

        [BindProperty] public string DisplayReferenceType { get; set; } = string.Empty;

        public IActionResult OnGet(string id)
        {
            Item = _configService.Get(id);
            if (Item == null) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });

            // Convert stored ReferenceType path back to display name
            DisplayReferenceType = Item.ReferenceType switch
            {
                "/Announcements" => "Announcement",
                "/Pictures/Details" => "Picture",
                "/Videos/Details" => "Video",
                _ => Item.ReferenceType // Page or other
            };

            // If ReferenceTitle is empty, try to populate it
            if (string.IsNullOrWhiteSpace(Item.ReferenceTitle) && !string.IsNullOrWhiteSpace(Item.ReferenceId))
            {
                Item.ReferenceTitle = GetReferenceTitle(DisplayReferenceType, Item.ReferenceId);
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Item == null) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
            if (string.IsNullOrWhiteSpace(DisplayReferenceType) || string.IsNullOrWhiteSpace(Item.ReferenceId))
            {
                ModelState.AddModelError(string.Empty, "Reference Type and Reference Value are required.");
                return Page();
            }
            if (!ModelState.IsValid) return Page();

            // Resolve ReferenceTitle and set proper ReferenceType path
            string? title = GetReferenceTitle(DisplayReferenceType, Item.ReferenceId);
            
            // Set proper ReferenceType path based on DisplayReferenceType
            if (string.Equals(DisplayReferenceType, "Page", StringComparison.OrdinalIgnoreCase))
            {
                Item.ReferenceType = "Page";
            }
            else if (string.Equals(DisplayReferenceType, "Announcement", StringComparison.OrdinalIgnoreCase))
            {
                Item.ReferenceType = "/Announcements";
            }
            else if (string.Equals(DisplayReferenceType, "Picture", StringComparison.OrdinalIgnoreCase))
            {
                Item.ReferenceType = "/Pictures/Details";
            }
            else if (string.Equals(DisplayReferenceType, "Video", StringComparison.OrdinalIgnoreCase))
            {
                Item.ReferenceType = "/Videos/Details";
            }

            Item.ReferenceTitle = title ?? Item.ReferenceTitle;
            Item.Timestamp = Item.Timestamp ?? DateTime.UtcNow;

            _configService.Update(Item);
            return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
        }

        private string? GetReferenceTitle(string displayReferenceType, string referenceId)
        {
            if (string.IsNullOrWhiteSpace(referenceId) || !int.TryParse(referenceId, out var id))
                return null;

            return displayReferenceType switch
            {
                "Page" => _pageService.Get(id)?.Title,
                "Announcement" => _announcementService.Get(id)?.Title,
                "Picture" => _pictureService.Get(id)?.Description,
                "Video" => _videoService.Get(id)?.Description,
                _ => null
            };
        }

        public JsonResult OnGetReferenceValues(string referenceType)
        {
            var list = new List<object>();
            if (string.Equals(referenceType, "Page", StringComparison.OrdinalIgnoreCase))
            {
                list = _pageService.GetAll()
                .OrderBy(p => p.Title)
                .Select(p => new { value = p.Id.ToString(), text = $"{p.Id} - {p.Title}" })
                .Cast<object>().ToList();
            }
            else if (string.Equals(referenceType, "Announcement", StringComparison.OrdinalIgnoreCase))
            {
                list = _announcementService.GetAll()
                .OrderBy(a => a.Title)
                .Select(a => new { value = a.Id.ToString(), text = $"{a.Id} - {a.Title}" })
                .Cast<object>().ToList();
            }
            else if (string.Equals(referenceType, "Picture", StringComparison.OrdinalIgnoreCase))
            {
                list = _pictureService.GetAll()
                .OrderBy(p => p.Description)
                .Select(p => new { value = p.Id.ToString(), text = $"{p.Id} - {p.Description}" })
                .Cast<object>().ToList();
            }
            else if (string.Equals(referenceType, "Video", StringComparison.OrdinalIgnoreCase))
            {
                list = _videoService.GetAll()
                .OrderBy(v => v.Description)
                .Select(v => new { value = v.Id.ToString(), text = $"{v.Id} - {v.Description}" })
                .Cast<object>().ToList();
            }
            return new JsonResult(list);
        }
    }
}
