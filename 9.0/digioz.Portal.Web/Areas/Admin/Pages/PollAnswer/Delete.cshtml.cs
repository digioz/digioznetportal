using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.PollAnswer
{
    public class DeleteModel : PageModel
    {
        private readonly IPollAnswerService _answerService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollVoteService _voteService;
        public DeleteModel(IPollAnswerService answerService, IPollUsersVoteService usersVoteService, IPollVoteService voteService)
        {
            _answerService = answerService;
            _usersVoteService = usersVoteService;
            _voteService = voteService;
        }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public string PollId { get; private set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(Id)) return BadRequest();
            var ans = _answerService.Get(Id);
            if (ans == null) return NotFound();
            PollId = ans.PollId;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return BadRequest();
            var ans = _answerService.Get(Id);
            if (ans == null) return NotFound();

            // delete votes referencing this answer
            foreach (var vote in _voteService.GetAll().FindAll(v => v.PollAnswerId == ans.Id))
            {
                _voteService.Delete(vote.Id);
            }

            // delete user vote rows for the poll this answer belongs to (users voted are tracked per poll)
            foreach (var uv in _usersVoteService.GetAll().FindAll(x => x.PollId == ans.PollId))
            {
                _usersVoteService.Delete(uv.PollId, uv.UserId);
            }

            _answerService.Delete(ans.Id);
            return RedirectToPage("Index", new { PollId = ans.PollId });
        }
    }
}
