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

        public PrivateMessage? Message { get; set; }
        public string FromDisplayName { get; set; } = string.Empty;

        public IActionResult OnGet(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            Message = _pmService.Get(id);
            if (Message == null || (Message.FromId != currentUserId && Message.ToId != currentUserId))
            {
                return NotFound();
            }
            var fromProfile = _profileService.GetAll()
                .FirstOrDefault(p => p.UserId == Message.FromId && !string.IsNullOrWhiteSpace(p.DisplayName));
            
            FromDisplayName = fromProfile?.DisplayName ?? "Unknown User";
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            // Perform atomic check and delete
            var deleted = _pmService.DeleteIfOwnedByUser(id, currentUserId);
            if (!deleted)
            {
                return NotFound();
            }
            return RedirectToPage("/PrivateMessages/Index");
        }
    }
}
