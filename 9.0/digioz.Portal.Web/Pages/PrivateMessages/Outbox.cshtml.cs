using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class OutboxModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public OutboxModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public class OutboxMessage
        {
            public int Id { get; set; }
            public string ToDisplayName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public System.DateTime? SentDate { get; set; }
        }

        public List<OutboxMessage> Messages { get; set; } = new();

        public void OnGet()
        {
            var currentUserId = _userManager.GetUserId(User);
            var outbox = _pmService.GetOutbox(currentUserId);
            var profileLookup = _profileService.GetAll()
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);

            Messages = outbox.Select(m => new OutboxMessage
            {
                Id = m.Id,
                ToDisplayName = profileLookup.GetValueOrDefault(m.ToId, "Unknown User"),
                Subject = m.Subject ?? "(No Subject)",
                SentDate = m.SentDate
            }).ToList();
        }
    }
}
