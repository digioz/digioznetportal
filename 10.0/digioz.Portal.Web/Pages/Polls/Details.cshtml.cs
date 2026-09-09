using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using digioz.Portal.Dal.Services.Interfaces;
using ScottPlot;

namespace digioz.Portal.Pages.Polls
{
    public class DetailsModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollVoteService _voteService;
        private readonly ILogger<DetailsModel> _logger;
        
        public DetailsModel(
            IPollService pollService, 
            IPollAnswerService answerService, 
            IPollUsersVoteService usersVoteService, 
            IPollVoteService voteService,
            ILogger<DetailsModel> logger)
        {
            _pollService = pollService;
            _answerService = answerService;
            _usersVoteService = usersVoteService;
            _voteService = voteService;
            _logger = logger;
        }

        public digioz.Portal.Bo.Poll Item { get; private set; } = new();
        public System.Collections.Generic.List<digioz.Portal.Bo.PollAnswer> Answers { get; private set; } = new();
        public bool HasVoted { get; private set; }
        public string ResultsChartBase64 { get; private set; } = string.Empty;
        [BindProperty] public System.Collections.Generic.List<string> SelectedAnswerIds { get; set; } = new();

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            bool isOwnPoll = !string.IsNullOrEmpty(userId) && poll.UserId == userId;
            bool isVisibleAndApproved = poll.Visible == true && poll.Approved == true;
            
            if (!isOwnPoll && !isVisibleAndApproved)
            {
                return NotFound();
            }
            
            Item = poll;
            Answers = _answerService.GetByPollId(id);
            HasVoted = !string.IsNullOrEmpty(userId) && _usersVoteService.Get(id, userId) != null;

            if (HasVoted)
            {
                ResultsChartBase64 = GenerateResultsChart(Answers);
            }

            return Page();
        }

        public IActionResult OnPostVote(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId)) return Forbid();
            
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            
            bool isOwnPoll = poll.UserId == userId;
            bool isVisibleAndApproved = poll.Visible == true && poll.Approved == true;
            
            // Users cannot vote on unapproved polls (including their own)
            if (!isVisibleAndApproved)
            {
                return NotFound();
            }
                       
            var prior = _usersVoteService.Get(id, userId);
            if (prior != null) return RedirectToPage(new { id });
            
            var validAnswers = _answerService.GetByPollId(id);
            var selected = (SelectedAnswerIds ?? new System.Collections.Generic.List<string>())
                .Where(s => validAnswers.Any(a => a.Id == s)).Distinct().ToList();
            
            if (!selected.Any())
            {
                ModelState.AddModelError(string.Empty, "Select at least one answer.");
                Item = poll;
                Answers = validAnswers;
                HasVoted = false;
                return Page();
            }
            
            if (!poll.AllowMultipleOptionsVote && selected.Count > 1)
                selected = selected.Take(1).ToList();
            
            _usersVoteService.Add(new digioz.Portal.Bo.PollUsersVote { PollId = id, UserId = userId, DateVoted = System.DateTime.UtcNow.ToString("o") });
            
            foreach (var ans in selected)
            {
                _voteService.Add(new digioz.Portal.Bo.PollVote { Id = System.Guid.NewGuid().ToString(), UserId = userId, PollAnswerId = ans });
            }
            
            return RedirectToPage(new { id });
        }

        private string GenerateResultsChart(List<digioz.Portal.Bo.PollAnswer> answers)
        {
            try
            {
                var counts = answers.Select(a => (double)_voteService.CountByAnswerId(a.Id)).ToArray();
                var labels = answers.Select(a => a.Answer ?? string.Empty).ToArray();
                if (labels.Length == 0) return string.Empty;

                using var plot = new Plot();
                var bars = plot.Add.Bars(counts);
                for (int i = 0; i < bars.Bars.Count; i++)
                {
                    bars.Bars[i].FillColor = ScottPlot.Color.FromHex("#0d6efd");
                    bars.Bars[i].Label = bars.Bars[i].Value.ToString("F0");
                }
                bars.ValueLabelStyle.Bold = true;
                bars.ValueLabelStyle.FontSize = 11;
                bars.ValueLabelStyle.ForeColor = ScottPlot.Color.FromHex("#212529");

                plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                    labels.Select((t, i) => new Tick((double)i, t)).ToArray()
                );
                plot.Axes.Bottom.Label.Text = "Answers";
                plot.Axes.Left.Label.Text = "Votes";
                plot.Title("Results");

                var bytes = plot.GetImage(500, 250).GetImageBytes();
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate poll results chart for poll with {AnswerCount} answers. Error: {ErrorMessage}", 
                    answers?.Count ?? 0, ex.Message);
                return string.Empty;
            }
        }
    }
}