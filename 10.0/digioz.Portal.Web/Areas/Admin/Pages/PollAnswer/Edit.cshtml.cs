using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using digioz.Portal.Web.Pages.Shared.Components.PollMenu;

namespace digioz.Portal.Web.Areas.Admin.Pages.PollAnswer
{
    public class EditModel : PageModel
    {
        private readonly IPollAnswerService _answerService;
        private readonly IPollService _pollService;
        private readonly IMemoryCache _cache;
        public EditModel(IPollAnswerService service, IPollService pollService, IMemoryCache cache)
        {
            _answerService = service;
            _pollService = pollService;
            _cache = cache;
        }

        [BindProperty] public digioz.Portal.Bo.PollAnswer Item { get; set; } = new();
        public System.Collections.Generic.List<digioz.Portal.Bo.Poll> Polls { get; private set; } = new();

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var item = _answerService.Get(id);
            if (item == null) return NotFound();
            Item = item;
            Polls = _pollService.GetLatest(50); // limit dropdown size
            return Page();
        }

        public IActionResult OnPost()
        {
            // Sanitize the answer text
            Item.Answer = InputSanitizer.SanitizePollAnswer(Item.Answer);
            
            if (!ModelState.IsValid)
            {
                Polls = _pollService.GetLatest(50);
                return Page();
            }
            
            _answerService.Update(Item);
            _cache.Remove(PollMenuViewComponent.GetPollCacheKey(Item.PollId));
            return RedirectToPage("Index", new { PollId = Item.PollId });
        }
    }
}
