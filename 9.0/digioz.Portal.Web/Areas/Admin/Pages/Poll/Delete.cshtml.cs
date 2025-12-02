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

            // targeted cascade delete
            var answerIds = _answerService.GetIdsByPollId(Id);
            _voteService.DeleteByPollId(Id, answerIds);
            _answerService.DeleteByPollId(Id);
            _usersVoteService.DeleteByPollId(Id);
            _pollService.Delete(Id);
            return RedirectToPage("Index");
        }
    }
}
