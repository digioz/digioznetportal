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
    public class EditModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        public EditModel(IPollService pollService, IPollAnswerService answerService)
        {
            _pollService = pollService;
            _answerService = answerService;
        }

        [BindProperty] public digioz.Portal.Bo.Poll Item { get; set; } = new();
        [BindProperty] public string NewAnswersCsv { get; set; } = string.Empty;

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (poll.UserId != userId) return Forbid();
            Item = poll;
            var existingAnswers = _answerService.GetAll().Where(a => a.PollId == id).Select(a => a.Answer).ToList();
            NewAnswersCsv = string.Join(", ", existingAnswers);
            return Page();
        }

        public IActionResult OnPost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(Item.Id)) return BadRequest();
            var poll = _pollService.Get(Item.Id);
            if (poll == null) return NotFound();
            if (poll.UserId != userId) return Forbid();
            if (!ModelState.IsValid) return Page();

            poll.Slug = Item.Slug;
            poll.IsClosed = Item.IsClosed;
            poll.Featured = Item.Featured;
            poll.AllowMultipleOptionsVote = Item.AllowMultipleOptionsVote;
            _pollService.Update(poll);

            // Sync answers (simple add-only avoiding duplicates; removal handled separately if needed)
            var requested = (NewAnswersCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var existing = _answerService.GetAll().Where(a => a.PollId == poll.Id).Select(a => a.Answer).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var ans in requested.Where(r => !existing.Contains(r)))
            {
                _answerService.Add(new digioz.Portal.Bo.PollAnswer { Id = Guid.NewGuid().ToString(), PollId = poll.Id, Answer = ans });
            }
            return RedirectToPage("/Polls/Index");
        }
    }
}
