using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Poll
{
    public class AddModel : PageModel
    {
        private readonly IPollService _service;
        private readonly IPollAnswerService _answerService;
        public AddModel(IPollService service, IPollAnswerService answerService)
        {
            _service = service;
            _answerService = answerService;
        }

        [BindProperty] public digioz.Portal.Bo.Poll Item { get; set; } = new digioz.Portal.Bo.Poll { Id = Guid.NewGuid().ToString(), DateCreated = DateTime.UtcNow };
        [BindProperty] public string NewAnswersCsv { get; set; } = string.Empty;

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (string.IsNullOrEmpty(Item.Id)) Item.Id = Guid.NewGuid().ToString();
            Item.DateCreated = DateTime.UtcNow;
            _service.Add(Item);

            // Add initial answers if provided (comma-separated)
            if (!string.IsNullOrWhiteSpace(NewAnswersCsv))
            {
                var answers = NewAnswersCsv
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var ans in answers)
                {
                    _answerService.Add(new digioz.Portal.Bo.PollAnswer
                    {
                        Id = Guid.NewGuid().ToString(),
                        PollId = Item.Id,
                        Answer = ans
                    });
                }
            }

            return RedirectToPage("Index");
        }
    }
}
