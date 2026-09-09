using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
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

        public void OnGet()
        {
            Polls = _pollService.GetLatest(50);
            if (!string.IsNullOrEmpty(PollId))
            {
                Item.PollId = PollId;
            }
        }

        public IActionResult OnPost()
        {
            // Sanitize the answer text
            Item.Answer = InputSanitizer.SanitizePollAnswer(Item.Answer);
            
            if (string.IsNullOrEmpty(Item.PollId))
            {
                ModelState.AddModelError(nameof(Item.PollId), "Please select a poll.");
            }
            
            if (string.IsNullOrWhiteSpace(Item.Answer))
            {
                ModelState.AddModelError(nameof(Item.Answer), "Please enter an answer.");
            }

            if (!ModelState.IsValid)
            {
                Polls = _pollService.GetLatest(50);
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
