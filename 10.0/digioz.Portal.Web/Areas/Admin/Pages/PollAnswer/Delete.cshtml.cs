using System.Linq;
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

            // Get all answers for this poll to determine which ones will remain
            var allAnswers = _answerService.GetByPollId(ans.PollId);
            var remainingAnswerIds = allAnswers
                .Where(a => a.Id != ans.Id)
                .Select(a => a.Id)
                .ToHashSet();

            // Get all votes for this poll (efficient targeted query)
            var allVotesForPoll = _voteService.GetByPollAnswerIds(allAnswers.Select(a => a.Id));
            
            // Find votes that will be deleted (votes for this answer only)
            var votesToDelete = allVotesForPoll.Where(v => v.PollAnswerId == ans.Id).ToList();
            var affectedUserIds = votesToDelete.Select(v => v.UserId).Distinct().ToList();

            // For each affected user, check if they have other votes for this poll
            foreach (var userId in affectedUserIds)
            {
                // Check if user has votes for other answers in this poll
                var hasOtherVotes = allVotesForPoll.Any(v => 
                    v.UserId == userId && remainingAnswerIds.Contains(v.PollAnswerId));
                
                // Only delete the user's vote record if they won't have any remaining votes
                if (!hasOtherVotes)
                {
                    _usersVoteService.Delete(ans.PollId, userId);
                }
            }

            // Delete votes for this specific answer (efficient database-level delete)
            _voteService.DeleteByAnswerId(ans.Id);

            // Finally, delete the answer itself
            _answerService.Delete(ans.Id);
            
            return RedirectToPage("Index", new { PollId = ans.PollId });
        }
    }
}
