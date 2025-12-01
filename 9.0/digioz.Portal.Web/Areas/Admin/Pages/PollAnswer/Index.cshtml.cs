using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.PollAnswer
{
    public class IndexModel : PageModel
    {
        private readonly IPollAnswerService _service;
        private readonly IPollService _pollService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollVoteService _voteService;
        public IndexModel(IPollAnswerService service, IPollService pollService, IPollUsersVoteService usersVoteService, IPollVoteService voteService)
        {
            _service = service; _pollService = pollService; _usersVoteService = usersVoteService; _voteService = voteService;
        }

        public IReadOnlyList<digioz.Portal.Bo.PollAnswer> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.PollAnswer>();
        [BindProperty(SupportsGet = true)] public string PollId { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public Dictionary<string, string> PollSlugs { get; private set; } = new();
        [BindProperty] public List<string> SelectedAnswerIds { get; set; } = new();
        public digioz.Portal.Bo.Poll? CurrentPoll { get; private set; }
        public bool HasVoted { get; private set; }

        public void OnGet()
        {
            // build poll slug lookup
            var polls = _pollService.GetAll();
            PollSlugs = polls.ToDictionary(p => p.Id, p => string.IsNullOrWhiteSpace(p.Slug) ? p.Id : p.Slug);
            if (!string.IsNullOrEmpty(PollId))
            {
                CurrentPoll = polls.FirstOrDefault(p => p.Id == PollId);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                HasVoted = !string.IsNullOrEmpty(userId) && _usersVoteService.Get(PollId, userId) != null;
            }

            var all = _service.GetAll();
            if (!string.IsNullOrEmpty(PollId))
                all = all.Where(a => a.PollId == PollId).ToList();

            TotalCount = all.Count;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();
        }

        public IActionResult OnPostVote()
        {
            if (string.IsNullOrEmpty(PollId)) return BadRequest();
            var poll = _pollService.Get(PollId);
            if (poll == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId)) return Forbid();

            // Block re-voting
            var priorVote = _usersVoteService.Get(PollId, userId);
            if (priorVote != null)
            {
                ModelState.AddModelError(string.Empty, "You have already voted for this poll.");
                OnGet();
                return Page();
            }

            var validAnswers = _service.GetAll().Where(a => a.PollId == PollId).ToList();
            var selected = (SelectedAnswerIds ?? new List<string>()).Where(s => validAnswers.Any(a => a.Id == s)).Distinct().ToList();

            if (!selected.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one answer.");
                OnGet();
                return Page();
            }
            if (!poll.AllowMultipleOptionsVote && selected.Count > 1)
                selected = selected.Take(1).ToList();

            _usersVoteService.Add(new digioz.Portal.Bo.PollUsersVote
            {
                PollId = PollId,
                UserId = userId,
                DateVoted = DateTime.UtcNow.ToString("o")
            });

            foreach (var ansId in selected)
            {
                _voteService.Add(new digioz.Portal.Bo.PollVote
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    PollAnswerId = ansId
                });
            }

            return RedirectToPage(new { PollId });
        }
    }
}
