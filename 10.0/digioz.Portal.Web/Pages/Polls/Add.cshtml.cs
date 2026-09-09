using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Polls
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        private readonly UserManager<IdentityUser> _userManager;
        
        public AddModel(IPollService pollService, IPollAnswerService answerService, UserManager<IdentityUser> userManager)
        {
            _pollService = pollService;
            _answerService = answerService;
            _userManager = userManager;
        }

        [BindProperty] public digioz.Portal.Bo.Poll Item { get; set; } = new digioz.Portal.Bo.Poll { Id = Guid.NewGuid().ToString(), DateCreated = DateTime.UtcNow };        
        [BindProperty] public string NewAnswersCsv { get; set; } = string.Empty;

        public void OnGet() { }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Forbid();
            
            // Sanitize poll question
            Item.Slug = InputSanitizer.SanitizePollQuestion(Item.Slug);
            
            if (!ModelState.IsValid) return Page();

            // Parse and sanitize answers
            var rawAnswers = NewAnswersCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();
            
            var sanitizedAnswers = InputSanitizer.SanitizePollAnswers(rawAnswers);
            
            // Validate minimum answer count
            var answerValidation = InputSanitizer.ValidateList(sanitizedAnswers, "answers", minCount: 2, maxCount: 50);
            if (answerValidation != null)
            {
                ModelState.AddModelError(nameof(NewAnswersCsv), answerValidation);
                return Page();
            }

            Item.Id = string.IsNullOrEmpty(Item.Id) ? Guid.NewGuid().ToString() : Item.Id;
            Item.DateCreated = DateTime.UtcNow;
            Item.UserId = userId;

            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Administrator");
            
            if (isAdmin)
            {
                Item.Visible = true;
                Item.Approved = true;
            }
            else
            {
                Item.Visible = false;
                Item.Approved = false;
            }

            _pollService.Add(Item);

            // Add sanitized answers
            foreach (var ans in sanitizedAnswers)
            {
                _answerService.Add(new digioz.Portal.Bo.PollAnswer 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    PollId = Item.Id, 
                    Answer = ans 
                });
            }

            return RedirectToPage("/Polls/Index");
        }
    }
}