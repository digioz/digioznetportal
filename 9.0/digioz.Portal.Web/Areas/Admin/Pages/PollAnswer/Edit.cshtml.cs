using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.PollAnswer
{
    public class EditModel : PageModel
    {
        private readonly IPollAnswerService _answerService;
        private readonly IPollService _pollService;
        public EditModel(IPollAnswerService service, IPollService pollService)
        {
            _answerService = service;
            _pollService = pollService;
        }

        [BindProperty] public digioz.Portal.Bo.PollAnswer Item { get; set; } = new();
        public System.Collections.Generic.List<digioz.Portal.Bo.Poll> Polls { get; private set; } = new();
        public System.Collections.Generic.List<digioz.Portal.Bo.PollAnswer> AnswersForPoll { get; private set; } = new();

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var item = _answerService.Get(id);
            if (item == null) return NotFound();
            Item = item;
            Polls = _pollService.GetLatest(50); // limit dropdown size
            AnswersForPoll = _answerService.GetByPollId(Item.PollId);
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Polls = _pollService.GetLatest(50);
                if (!string.IsNullOrEmpty(Item.PollId))
                    AnswersForPoll = _answerService.GetByPollId(Item.PollId);
                return Page();
            }
            _answerService.Update(Item);
            return RedirectToPage("Index", new { PollId = Item.PollId });
        }
    }
}
