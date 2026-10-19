using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.ReportedMessage
{
    public class DetailsModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;

        public DetailsModel(IPrivateMessageService pmService, IProfileService profileService)
        {
            _pmService = pmService;
            _profileService = profileService;
        }

        public class ThreadMessage
        {
            public int Id { get; set; }
            public string FromDisplayName { get; set; } = string.Empty;
            public string ToDisplayName { get; set; } = string.Empty;
            public DateTime? SentDate { get; set; }
            public string? Subject { get; set; }
            public string? Message { get; set; }
            public bool IsRoot { get; set; }
        }

        public PrivateMessage? RootMessage { get; set; }
        public string FromDisplayName { get; set; } = string.Empty;
        public string ToDisplayName { get; set; } = string.Empty;
        public List<ThreadMessage> Thread { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            RootMessage = _pmService.Get(id);
            if (RootMessage == null || !RootMessage.Reported)
            {
                return RedirectToPage("/ReportedMessage/Index", new { area = "Admin" });
            }

            var rawThread = _pmService.GetThread(id);

            var userIds = rawThread
                .SelectMany(m => new[] { m.FromId, m.ToId })
                .Where(uid => !string.IsNullOrWhiteSpace(uid))
                .Distinct()
                .ToList();
            var profiles = _profileService.GetByUserIds(userIds);
            var profileLookup = profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);

            FromDisplayName = profileLookup.GetValueOrDefault(RootMessage.FromId, "Unknown User");
            ToDisplayName = profileLookup.GetValueOrDefault(RootMessage.ToId, "Unknown User");

            Thread = rawThread.Select(m => new ThreadMessage
            {
                Id = m.Id,
                FromDisplayName = profileLookup.GetValueOrDefault(m.FromId, "Unknown User"),
                ToDisplayName = profileLookup.GetValueOrDefault(m.ToId, "Unknown User"),
                SentDate = m.SentDate,
                Subject = m.Subject,
                Message = m.Message,
                IsRoot = m.Id == RootMessage.Id
            }).ToList();

            return Page();
        }

        public IActionResult OnPostRemoveReport(int id)
        {
            _pmService.RemoveReport(id);
            return RedirectToPage("/ReportedMessage/Index", new { area = "Admin" });
        }
    }
}
