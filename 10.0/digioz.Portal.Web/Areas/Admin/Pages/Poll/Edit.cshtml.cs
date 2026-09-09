using System;
using System.Linq;
using System.Collections.Generic;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
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
            ExistingAnswers = _answerService.GetByPollId(id);
            return Page();
        }

        public IActionResult OnPost()
        {
            Item.Slug = InputSanitizer.SanitizePollQuestion(Item.Slug);
            
            if (!ModelState.IsValid) return Page();
            
            // Get existing poll to preserve DateCreated and other fields
            var existing = _service.Get(Item.Id);
            if (existing == null) return NotFound();
            
            // Update only the editable fields
            existing.Slug = Item.Slug;
            existing.UserId = Item.UserId;
            existing.Featured = Item.Featured;
            existing.IsClosed = Item.IsClosed;
            existing.AllowMultipleOptionsVote = Item.AllowMultipleOptionsVote;
            existing.Visible = Item.Visible == true ? true : false;
            existing.Approved = Item.Approved == true ? true : false;
            // DateCreated is preserved from existing poll
            
            _service.Update(existing);

            string Norm(string s) => (s ?? string.Empty).Trim();
            string Key(string s) => Norm(s).ToLowerInvariant();

            var rawAnswers = (NewAnswersCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();
            
            var requested = InputSanitizer.SanitizePollAnswers(rawAnswers);
            
            var answerValidation = InputSanitizer.ValidateList(requested, "answers", minCount: 2, maxCount: 50);
            if (answerValidation != null)
            {
                ModelState.AddModelError(nameof(NewAnswersCsv), answerValidation);
                ExistingAnswers = _answerService.GetByPollId(Item.Id);
                return Page();
            }

            var existingAnswers = _answerService.GetByPollId(Item.Id);

            var dupIds = new List<string>();
            foreach (var grp in existingAnswers.GroupBy(a => Key(a.Answer)))
            {
                var keep = grp.OrderBy(a => a.Id).First();
                dupIds.AddRange(grp.Where(a => a.Id != keep.Id).Select(a => a.Id));
            }

            var toRemoveByRequest = existingAnswers
                .Where(e => requested.Count > 0 && !requested.Contains(Norm(e.Answer), StringComparer.OrdinalIgnoreCase))
                .Select(e => e.Id)
                .ToList();

            var answersToRemoveIds = dupIds
                .Concat(toRemoveByRequest)
                .Distinct()
                .ToList();

            var answersToKeepIds = existingAnswers.Select(a => a.Id).Except(answersToRemoveIds).ToHashSet();

            var votesForPoll = _voteService.GetByPollAnswerIds(existingAnswers.Select(a => a.Id));
            var votesToRemovedAnswers = votesForPoll.Where(v => answersToRemoveIds.Contains(v.PollAnswerId)).ToList();
            var affectedUserIds = votesToRemovedAnswers.Select(v => v.UserId).Distinct().ToList();

            foreach (var uid in affectedUserIds)
            {
                bool hasOtherVote = votesForPoll.Any(v => v.UserId == uid && answersToKeepIds.Contains(v.PollAnswerId));
                if (!hasOtherVote)
                {
                    _usersVoteService.Delete(Item.Id, uid);
                }
            }

            foreach (var ansId in answersToRemoveIds)
            {
                _voteService.DeleteByAnswerId(ansId);
                _answerService.Delete(ansId);
            }

            var keptNormalized = existingAnswers
                .Where(a => answersToKeepIds.Contains(a.Id))
                .Select(a => Norm(a.Answer))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var text in requested.Where(r => !keptNormalized.Contains(r)))
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
