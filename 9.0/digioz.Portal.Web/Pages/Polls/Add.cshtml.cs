using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        public AddModel(IPollService pollService, IPollAnswerService answerService)
        {
            _pollService = pollService;
            _answerService = answerService;
        }

        [BindProperty] public digioz.Portal.Bo.Poll Item { get; set; } = new digioz.Portal.Bo.Poll { Id = Guid.NewGuid().ToString(), DateCreated = DateTime.UtcNow };        
        [BindProperty] public string NewAnswersCsv { get; set; } = string.Empty;

        public void OnGet() { }

        public IActionResult OnPost()
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