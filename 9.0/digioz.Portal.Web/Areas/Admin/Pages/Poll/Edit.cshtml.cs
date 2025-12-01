using System;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Poll
{
    public class EditModel : PageModel
    {
        private readonly IPollService _service;
        private readonly IPollAnswerService _answerService;
        private readonly IPollVoteService _voteService;
        private readonly IPollUsersVoteService _usersVoteService;
        public EditModel(IPollService service, IPollAnswerService answerService, IPollVoteService voteService, IPollUsersVoteService usersVoteService)
        {
            _service = service;
            _answerService = answerService;
            _voteService = voteService;
            _usersVoteService = usersVoteService;
        }

        [BindProperty] public digioz.Portal.Bo.Poll Item { get; set; } = new digioz.Portal.Bo.Poll();
        [BindProperty] public string NewAnswersCsv { get; set; } = string.Empty;
        public System.Collections.Generic.List<digioz.Portal.Bo.PollAnswer> ExistingAnswers { get; private set; } = new();

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var item = _service.Get(id);
            if (item == null) return NotFound();
            Item = item;
            ExistingAnswers = _answerService.GetAll().Where(a => a.PollId == id).ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            _service.Update(Item);

            // Normalize helper
            string Norm(string s) => (s ?? string.Empty).Trim();
            string Key(string s) => Norm(s).ToLowerInvariant();

            // Parse requested answers from CSV
            var requested = (NewAnswersCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => Norm(a))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existing = _answerService.GetAll().Where(a => a.PollId == Item.Id).ToList();

            // Collapse duplicates in existing: keep one per normalized text, remove extras
            var groups = existing
                .GroupBy(a => Key(a.Answer))
                .ToList();

            foreach (var grp in groups)
            {
                // keep first (stable by Id)
                var keep = grp.OrderBy(a => a.Id).FirstOrDefault();
                foreach (var dup in grp.Where(a => a != keep))
                {
                    foreach (var vote in _voteService.GetAll().FindAll(v => v.PollAnswerId == dup.Id))
                        _voteService.Delete(vote.Id);
                    foreach (var uv in _usersVoteService.GetAll().FindAll(x => x.PollId == Item.Id))
                        _usersVoteService.Delete(uv.PollId, uv.UserId);
                    _answerService.Delete(dup.Id);
                }
            }

            // Refresh existing after dedupe
            existing = _answerService.GetAll().Where(a => a.PollId == Item.Id).ToList();
            var existingSet = existing.Select(e => Norm(e.Answer)).Where(a => !string.IsNullOrWhiteSpace(a))
                                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Remove answers not requested anymore
            var toRemove = existing.Where(e => requested.Count > 0 && !requested.Contains(Norm(e.Answer), StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var ans in toRemove)
            {
                foreach (var vote in _voteService.GetAll().FindAll(v => v.PollAnswerId == ans.Id))
                    _voteService.Delete(vote.Id);
                foreach (var uv in _usersVoteService.GetAll().FindAll(x => x.PollId == Item.Id))
                    _usersVoteService.Delete(uv.PollId, uv.UserId);
                _answerService.Delete(ans.Id);
            }

            // Add new answers
            foreach (var text in requested.Where(r => !existingSet.Contains(r)))
            {
                _answerService.Add(new digioz.Portal.Bo.PollAnswer
                {
                    Id = Guid.NewGuid().ToString(),
                    PollId = Item.Id,
                    Answer = text
                });
            }

            return RedirectToPage("Index");
        }
    }
}
