using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Poll
{
    public class DeleteModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollVoteService _voteService;
        public DeleteModel(IPollService pollService, IPollAnswerService answerService, IPollUsersVoteService usersVoteService, IPollVoteService voteService)
        {
            _pollService = pollService;
            _answerService = answerService;
            _usersVoteService = usersVoteService;
            _voteService = voteService;
        }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(Id)) return BadRequest();
            var poll = _pollService.Get(Id);
            if (poll == null) return NotFound();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return BadRequest();

            // delete answers and votes related to this poll
            var answers = _answerService.GetAll().FindAll(a => a.PollId == Id);
            foreach (var ans in answers)
            {
                // delete poll votes for this answer
                foreach (var vote in _voteService.GetAll().FindAll(v => v.PollAnswerId == ans.Id))
                {
                    _voteService.Delete(vote.Id);
                }
                _answerService.Delete(ans.Id);
            }

            // delete users' votes record for this poll
            foreach (var uv in _usersVoteService.GetAll().FindAll(x => x.PollId == Id))
            {
                _usersVoteService.Delete(uv.PollId, uv.UserId);
            }

            _pollService.Delete(Id);
            return RedirectToPage("Index");
        }
    }
}
