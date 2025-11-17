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
        public IndexModel(IChatService chatService) { _chatService = chatService; }

        public IReadOnlyList<digioz.Portal.Bo.Chat> History { get; private set; } = new List<digioz.Portal.Bo.Chat>();
        public string? CurrentUserName { get; private set; }
        public string? CurrentUserId { get; private set; }

        public void OnGet() {
            CurrentUserName = User.Identity?.Name;
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            History = _chatService.GetAll().OrderByDescending(c => c.Id).Take(50).ToList();
        }
    }
}