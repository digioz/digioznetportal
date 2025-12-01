using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Pages.Polls
{
    public class DetailsModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollVoteService _voteService;
        public DetailsModel(IPollService pollService, IPollAnswerService answerService, IPollUsersVoteService usersVoteService, IPollVoteService voteService)
        {
            _pollService = pollService;
            _answerService = answerService;
            _usersVoteService = usersVoteService;
            _voteService = voteService;
        }

        public digioz.Portal.Bo.Poll Item { get; private set; } = new();
        public System.Collections.Generic.List<digioz.Portal.Bo.PollAnswer> Answers { get; private set; } = new();
        public bool HasVoted { get; private set; }
        [BindProperty] public System.Collections.Generic.List<string> SelectedAnswerIds { get; set; } = new();

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            Item = poll;
            Answers = _answerService.GetByPollId(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            HasVoted = !string.IsNullOrEmpty(userId) && _usersVoteService.Get(id, userId) != null;
            return Page();
        }

        public IActionResult OnPostVote(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId)) return Forbid();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            var prior = _usersVoteService.Get(id, userId);
            if (prior != null) return RedirectToPage(new { id });
            var validAnswers = _answerService.GetByPollId(id);
            var selected = (SelectedAnswerIds ?? new System.Collections.Generic.List<string>())
                .Where(s => validAnswers.Any(a => a.Id == s)).Distinct().ToList();
            if (!selected.Any())
            {
                ModelState.AddModelError(string.Empty, "Select at least one answer.");
                Item = poll;
                Answers = validAnswers;
                HasVoted = false;
                return Page();
            }
            if (!poll.AllowMultipleOptionsVote && selected.Count > 1)
                selected = selected.Take(1).ToList();
            _usersVoteService.Add(new digioz.Portal.Bo.PollUsersVote { PollId = id, UserId = userId, DateVoted = System.DateTime.UtcNow.ToString("o") });
            foreach (var ans in selected)
            {
                _voteService.Add(new digioz.Portal.Bo.PollVote { Id = System.Guid.NewGuid().ToString(), UserId = userId, PollAnswerId = ans });
            }
            return RedirectToPage(new { id });
        }
    }
}