using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using digioz.Portal.Dal.Services.Interfaces;
using System.Security.Claims;
using System.Linq;

namespace digioz.Portal.Pages.Chat {
    [Authorize]
    public class IndexModel : PageModel {
        private readonly IChatService _chatService;
        private readonly IProfileService _profileService;
        
        public IndexModel(IChatService chatService, IProfileService profileService) 
        { 
            _chatService = chatService; 
            _profileService = profileService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Chat> History { get; private set; } = new List<digioz.Portal.Bo.Chat>();
        public Dictionary<string, string> DisplayNames { get; private set; } = new Dictionary<string, string>();
        public string? CurrentDisplayName { get; private set; }
        public string? CurrentUserId { get; private set; }

        public void OnGet() {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get current user's display name
            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                var currentProfile = _profileService.GetAll().FirstOrDefault(p => p.UserId == CurrentUserId);
                CurrentDisplayName = currentProfile?.DisplayName ?? User.Identity?.Name ?? "Anonymous";
            }
            else
            {
                CurrentDisplayName = User.Identity?.Name ?? "Anonymous";
            }
            
            // Get messages in ascending order (oldest first)
            History = _chatService.GetAll()
                .OrderBy(c => c.Id)
                .Take(50)
                .ToList();
            
            // Build a dictionary of userId -> displayName for display
            var userIds = History.Select(c => c.UserId).Distinct().ToList();
            var profiles = _profileService.GetAll().Where(p => userIds.Contains(p.UserId)).ToList();
            DisplayNames = profiles.ToDictionary(
                p => p.UserId, 
                p => p.DisplayName ?? p.Email ?? "Anonymous"
            );
            
            // For users without profiles, add fallback
            foreach (var userId in userIds.Where(id => !DisplayNames.ContainsKey(id)))
            {
                DisplayNames[userId] = "Anonymous";
            }
        }
    }
}