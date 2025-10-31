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
        public EditModel(ICommentConfigService configService, IPageService pageService, IAnnouncementService announcementService)
        {
            _configService = configService;
            _pageService = pageService;
            _announcementService = announcementService;
        }

        [BindProperty] public Bo.CommentConfig? Item { get; set; }

        public IActionResult OnGet(string id)
        {
            Item = _configService.Get(id);
            if (Item == null) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Item == null) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
            if (string.IsNullOrWhiteSpace(Item.ReferenceType) || string.IsNullOrWhiteSpace(Item.ReferenceId))
            {
                ModelState.AddModelError(string.Empty, "Reference Type and Reference Value are required.");
                return Page();
            }
            if (!ModelState.IsValid) return Page();

            // Resolve ReferenceTitle
            string? title = null;
            if (string.Equals(Item.ReferenceType, "Page", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(Item.ReferenceId, out var pid))
                    title = _pageService.Get(pid)?.Title;
            }
            else if (string.Equals(Item.ReferenceType, "Announcement", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(Item.ReferenceId, out var aid))
                    title = _announcementService.Get(aid)?.Title;
            }
            Item.ReferenceTitle = title ?? Item.ReferenceTitle;
            Item.Timestamp = Item.Timestamp ?? DateTime.UtcNow;

            _configService.Update(Item);
            return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
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
            return new JsonResult(list);
        }
    }
}
