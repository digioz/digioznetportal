using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class SentModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public SentModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public class SentMessage
        {
            public int Id { get; set; }
            public string ToDisplayName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public System.DateTime? SentDate { get; set; }
        }

        public List<SentMessage> Messages { get; set; } = new();

        public void OnGet()
        {
            var currentUserId = _userManager.GetUserId(User);
            var sent = _pmService.GetSent(currentUserId);
            var toIds = sent.Select(m => m.ToId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var profiles = _profileService.GetByUserIds(toIds);
            var profileLookup = profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);
            
            Messages = sent.Select(m => new SentMessage
            {
                Id = m.Id,
                ToDisplayName = profileLookup.GetValueOrDefault(m.ToId, "Unknown User"),
                Subject = m.Subject ?? "(No Subject)",
                SentDate = m.SentDate
            }).ToList();
        }
    }
}
