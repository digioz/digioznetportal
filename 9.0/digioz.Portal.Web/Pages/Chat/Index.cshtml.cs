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
        private readonly IAspNetUserService _userService;
        
        public IndexModel(IChatService chatService, IAspNetUserService userService) 
        { 
            _chatService = chatService; 
            _userService = userService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Chat> History { get; private set; } = new List<digioz.Portal.Bo.Chat>();
        public Dictionary<string, string> UserNames { get; private set; } = new Dictionary<string, string>();
        public string? CurrentUserName { get; private set; }
        public string? CurrentUserId { get; private set; }

        public void OnGet() {
            CurrentUserName = User.Identity?.Name;
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get messages in ascending order (oldest first)
            History = _chatService.GetAll()
                .OrderBy(c => c.Id)
                .Take(50)
                .ToList();
            
            // Build a dictionary of userId -> userName for display
            var userIds = History.Select(c => c.UserId).Distinct().ToList();
            var users = _userService.GetAll().Where(u => userIds.Contains(u.Id)).ToList();
            UserNames = users.ToDictionary(u => u.Id, u => u.UserName ?? u.Email ?? "Unknown");
        }
    }
}