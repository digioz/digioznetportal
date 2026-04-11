using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.ReportedMessage
{
    public class IndexModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;

        public IndexModel(IPrivateMessageService pmService, IProfileService profileService)
        {
            _pmService = pmService;
            _profileService = profileService;
        }

        public class ReportedMessageItem
        {
            public int Id { get; set; }
            public string FromDisplayName { get; set; } = string.Empty;
            public string ToDisplayName { get; set; } = string.Empty;
            public string? Subject { get; set; }
            public string? Message { get; set; }
            public DateTime? SentDate { get; set; }
        }

        public IReadOnlyList<ReportedMessageItem> Items { get; private set; } = Array.Empty<ReportedMessageItem>();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalCount { get; private set; }

        public void OnGet()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;

            var reported = _pmService.GetReported(PageNumber, PageSize, out var total);
            TotalCount = total;

            var userIds = reported
                .SelectMany(m => new[] { m.FromId, m.ToId })
                .Where(uid => !string.IsNullOrWhiteSpace(uid))
                .Distinct()
                .ToList();
            var profiles = _profileService.GetByUserIds(userIds);
            var profileLookup = profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);

            Items = reported.Select(m => new ReportedMessageItem
            {
                Id = m.Id,
                FromDisplayName = profileLookup.GetValueOrDefault(m.FromId, "Unknown User"),
                ToDisplayName = profileLookup.GetValueOrDefault(m.ToId, "Unknown User"),
                Subject = m.Subject,
                Message = m.Message,
                SentDate = m.SentDate
            }).ToList();
        }

        public IActionResult OnPostRemoveReport(int id)
        {
            _pmService.RemoveReport(id);
            return RedirectToPage(new { PageNumber, PageSize });
        }
    }
}
