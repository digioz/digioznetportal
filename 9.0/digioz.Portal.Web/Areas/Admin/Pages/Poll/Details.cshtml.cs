using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScottPlot;

namespace digioz.Portal.Web.Areas.Admin.Pages.Poll
{
    public class DetailsModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollVoteService _voteService;
        private readonly IPollUsersVoteService _usersVoteService;
        public DetailsModel(IPollService pollService, IPollAnswerService answerService, IPollVoteService voteService, IPollUsersVoteService usersVoteService)
        {
            _pollService = pollService;
            _answerService = answerService;
            _voteService = voteService;
            _usersVoteService = usersVoteService;
        }

        public digioz.Portal.Bo.Poll Item { get; private set; } = new();
        public string ResultsChartBase64 { get; private set; } = string.Empty;
        public List<digioz.Portal.Bo.PollAnswer> Answers { get; private set; } = new();
        [BindProperty] public List<string> SelectedAnswerIds { get; set; } = new();
        public bool HasVoted { get; private set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();
            Item = poll;
            Answers = _answerService.GetByPollId(id);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            HasVoted = !string.IsNullOrEmpty(userId) && _usersVoteService.Get(id, userId) != null;

            ResultsChartBase64 = GenerateResultsChart(id);
            return Page();
        }

        public IActionResult OnPostVote(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();
            var poll = _pollService.Get(id);
            if (poll == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId)) return Forbid();

            var validAnswers = _answerService.GetByPollId(id);

            // Block re-voting
            var priorVote = _usersVoteService.Get(id, userId);
            if (priorVote != null)
            {
                ModelState.AddModelError(string.Empty, "You have already voted for this poll.");
                Item = poll;
                Answers = validAnswers;
                HasVoted = true;
                ResultsChartBase64 = GenerateResultsChart(id);
                return Page();
            }

            var selected = (SelectedAnswerIds ?? new List<string>()).Where(s => validAnswers.Any(a => a.Id == s)).Distinct().ToList();
            if (!selected.Any())
            {
                ModelState.AddModelError(string.Empty, "Please select at least one answer.");
                Item = poll;
                Answers = validAnswers;
                ResultsChartBase64 = GenerateResultsChart(id);
                return Page();
            }
            if (!poll.AllowMultipleOptionsVote && selected.Count > 1)
            {
                selected = selected.Take(1).ToList();
            }

            // Record user vote first
            _usersVoteService.Add(new digioz.Portal.Bo.PollUsersVote
            {
                PollId = id,
                UserId = userId,
                DateVoted = DateTime.UtcNow.ToString("o")
            });

            // Add votes
            foreach (var ansId in selected)
            {
                _voteService.Add(new digioz.Portal.Bo.PollVote
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    PollAnswerId = ansId
                });
            }

            return RedirectToPage(new { id });
        }

        private string GenerateResultsChart(string pollId)
        {
            try
            {
                var answers = _answerService.GetByPollId(pollId);
                var counts = answers.Select(a => (double)_voteService.CountByAnswerId(a.Id)).ToArray();
                var labels = answers.Select(a => a.Answer).ToArray();

                using var plot = new Plot();
                var bars = plot.Add.Bars(counts);
                for (int i = 0; i < bars.Bars.Count; i++)
                {
                    bars.Bars[i].FillColor = ScottPlot.Color.FromHex("#0d6efd");
                    bars.Bars[i].Label = bars.Bars[i].Value.ToString("F0");
                }
                bars.ValueLabelStyle.Bold = true;
                bars.ValueLabelStyle.FontSize = 12;
                bars.ValueLabelStyle.ForeColor = ScottPlot.Color.FromHex("#212529");

                plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
                    labels.Select((t, i) => new Tick((double)i, t)).ToArray()
                );
                plot.Axes.Bottom.Label.Text = "Answers";
                plot.Axes.Left.Label.Text = "Votes";
                plot.Title("Poll Results");

                var bytes = plot.GetImage(800, 400).GetImageBytes();
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
