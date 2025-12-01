using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.PollAnswer
{
    public class AddModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        public AddModel(IPollAnswerService answerService, IPollService pollService)
        {
            _answerService = answerService;
            _pollService = pollService;
        }

        [BindProperty] public digioz.Portal.Bo.PollAnswer Item { get; set; } = new digioz.Portal.Bo.PollAnswer { Id = System.Guid.NewGuid().ToString() };
        [BindProperty(SupportsGet = true)] public string PollId { get; set; } = string.Empty;
        public System.Collections.Generic.List<digioz.Portal.Bo.Poll> Polls { get; private set; } = new();
        public System.Collections.Generic.List<digioz.Portal.Bo.PollAnswer> AnswersForPoll { get; private set; } = new();

        public void OnGet()
        {
            Polls = _pollService.GetLatest(50);
            if (!string.IsNullOrEmpty(PollId))
            {
                Item.PollId = PollId;
                AnswersForPoll = _answerService.GetByPollId(PollId);
            }
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Item.PollId))
            {
                ModelState.AddModelError(string.Empty, "Please select a poll.");
            }
            if (string.IsNullOrWhiteSpace(Item.Answer))
            {
                ModelState.AddModelError(string.Empty, "Please enter an answer.");
            }

            if (!ModelState.IsValid)
            {
                Polls = _pollService.GetLatest(50);
                if (!string.IsNullOrEmpty(Item.PollId))
                    AnswersForPoll = _answerService.GetByPollId(Item.PollId);
                return Page();
            }

            if (string.IsNullOrEmpty(Item.Id))
            {
                Item.Id = System.Guid.NewGuid().ToString();
            }

            _answerService.Add(Item);
            return RedirectToPage("Index", new { PollId = Item.PollId });
        }
    }
}
