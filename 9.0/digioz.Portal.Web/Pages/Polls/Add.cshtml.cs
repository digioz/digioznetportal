using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

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
            if (!ModelState.IsValid) return Page();

            Item.Id = string.IsNullOrEmpty(Item.Id) ? Guid.NewGuid().ToString() : Item.Id;
            Item.DateCreated = DateTime.UtcNow;
            Item.UserId = userId;
            _pollService.Add(Item);

            if (!string.IsNullOrWhiteSpace(NewAnswersCsv))
            {
                var answers = NewAnswersCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                foreach (var ans in answers)
                {
                    _answerService.Add(new digioz.Portal.Bo.PollAnswer { Id = Guid.NewGuid().ToString(), PollId = Item.Id, Answer = ans });
                }
            }

            return RedirectToPage("/Polls/Index");
        }
    }
}