using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Pages.Polls
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollVoteService _voteService;
        private readonly IPollUsersVoteService _usersVoteService;
        public DeleteModel(IPollService pollService, IPollAnswerService answerService, IPollVoteService voteService, IPollUsersVoteService usersVoteService)
        {
            _pollService = pollService;
            _answerService = answerService;
            _voteService = voteService;
            _usersVoteService = usersVoteService;
        }

        public digioz.Portal.Bo.Poll Item { get; private set; } = new();

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (poll.UserId != userId) return Forbid();
            Item = poll;
            return Page();
        }

        public IActionResult OnPost(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (poll.UserId != userId) return Forbid();

            // Cascade delete answers, votes, user votes
            var answers = _answerService.GetAll().Where(a => a.PollId == id).ToList();
            foreach (var ans in answers)
            {
                var votes = _voteService.GetAll().Where(v => v.PollAnswerId == ans.Id).ToList();
                foreach (var v in votes) _voteService.Delete(v.Id);
                _answerService.Delete(ans.Id);
            }
            foreach (var uv in _usersVoteService.GetAll().Where(x => x.PollId == id).ToList())
            {
                _usersVoteService.Delete(uv.PollId, uv.UserId);
            }
            _pollService.Delete(id);
            return RedirectToPage("/Polls/Index");
        }
    }
}
