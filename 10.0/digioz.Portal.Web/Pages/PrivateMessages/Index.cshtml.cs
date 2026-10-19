using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public class InboxMessage
        {
            public int Id { get; set; }
            public string FromDisplayName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public System.DateTime? SentDate { get; set; }
            public bool IsRead { get; set; }
        }

        public List<InboxMessage> Messages { get; set; } = new();

        public void OnGet()
        {
            var currentUserId = _userManager.GetUserId(User);
            var inbox = _pmService.GetInbox(currentUserId);
            var fromUserIds = inbox.Select(m => m.FromId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var profiles = _profileService.GetByUserIds(fromUserIds);
            var profileLookup = profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);
            
            Messages = inbox.Select(m => new InboxMessage
            {
                Id = m.Id,
                FromDisplayName = profileLookup.GetValueOrDefault(m.FromId, "Unknown User"),
                Subject = m.Subject ?? "(No Subject)",
                SentDate = m.SentDate,
                IsRead = m.IsRead
            }).ToList();
        }
    }
}
