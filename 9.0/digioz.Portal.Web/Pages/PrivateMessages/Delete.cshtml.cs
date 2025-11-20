using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;
using System.Linq;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public PrivateMessage Message { get; set; }
        public string FromDisplayName { get; set; } = string.Empty;

        public IActionResult OnGet(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            Message = _pmService.Get(id);
            if (Message == null || (Message.FromId != currentUserId && Message.ToId != currentUserId))
            {
                return NotFound();
            }
            var profileLookup = _profileService.GetAll()
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);
            
            FromDisplayName = profileLookup.GetValueOrDefault(Message.FromId, "Unknown User");
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            // Re-check ownership on POST to be safe
            var messageToDelete = _pmService.Get(id);
            if (messageToDelete == null || (messageToDelete.FromId != currentUserId && messageToDelete.ToId != currentUserId))
            {
                return NotFound();
            }
            _pmService.Delete(id, currentUserId);
            return RedirectToPage("/PrivateMessages/Index");
        }
    }
}
