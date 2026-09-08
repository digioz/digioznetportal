using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using digioz.Portal.Dal.Services.Interfaces;
using ScottPlot;
using digioz.Portal.Bo.ViewModels;

namespace digioz.Portal.Web.Pages.Shared.Components.PollMenu
{
    public class PollMenuViewComponent : ViewComponent
    {
        private readonly IPollService _pollService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollVoteService _voteService;
        private readonly IPluginService _pluginService;
        private readonly ILogger<PollMenuViewComponent> _logger;

        public PollMenuViewComponent(
            IPollService pollService, 
            IPollUsersVoteService usersVoteService, 
            IPollAnswerService answerService, 
            IPollVoteService voteService, 
            IPluginService pluginService,
            ILogger<PollMenuViewComponent> logger)
        {
            _pollService = pollService;
            _usersVoteService = usersVoteService;
            _answerService = answerService;
            _voteService = voteService;
            _pluginService = pluginService;
            _logger = logger;
        }

        public IViewComponentResult Invoke()
        {
            var pollPlugin = _pluginService.GetByName("Polls");
            if (pollPlugin == null || !pollPlugin.IsEnabled)
            {
                return View("Disabled");
            }

            var polls = _pollService.GetLatestFeatured(2);
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var model = new List<PollMenuItemViewModel>();
            foreach (var p in polls)
            {
                var answers = _answerService.GetByPollId(p.Id);
                var hasVoted = !string.IsNullOrEmpty(userId) && _usersVoteService.Exists(p.Id, userId);
                var chart = GenerateResultsChart(p.Id, answers);
                model.Add(new PollMenuItemViewModel
                {
                    Poll = p,
                    Answers = answers,
                    HasVoted = hasVoted,
                    ResultsChartBase64 = chart
                });
            }

            return View(model);
        }

        private string GenerateResultsChart(string pollId, List<digioz.Portal.Bo.PollAnswer> answers)
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
                _logger.LogError(ex, "Failed to generate poll results chart for poll {PollId} with {AnswerCount} answers. Error: {ErrorMessage}", 
                    pollId, answers?.Count ?? 0, ex.Message);
                return string.Empty;
            }
        }
    }
}
