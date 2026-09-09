using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using digioz.Portal.Dal.Services.Interfaces;
using ScottPlot;
using digioz.Portal.Bo.ViewModels;

namespace digioz.Portal.Web.Pages.Shared.Components.PollMenu
{
    public class PollMenuViewComponent : ViewComponent
    {
        private const string PluginCacheKey = "PollMenu_PluginEnabled";
        private const string FeaturedPollsCacheKey = "PollMenu_FeaturedPolls";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Cache key for a single poll's answers and rendered results chart.
        /// Remove this key when votes are cast so results refresh immediately.
        /// </summary>
        public static string GetPollCacheKey(string pollId) => $"PollMenu_Poll_{pollId}";

        private sealed class CachedPollData
        {
            public List<digioz.Portal.Bo.PollAnswer> Answers { get; init; } = new();
            public string ChartBase64 { get; init; } = string.Empty;
        }

        private readonly IPollService _pollService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollAnswerService _answerService;
        private readonly IPollVoteService _voteService;
        private readonly IPluginService _pluginService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PollMenuViewComponent> _logger;

        public PollMenuViewComponent(
            IPollService pollService, 
            IPollUsersVoteService usersVoteService, 
            IPollAnswerService answerService, 
            IPollVoteService voteService, 
            IPluginService pluginService,
            IMemoryCache cache,
            ILogger<PollMenuViewComponent> logger)
        {
            _pollService = pollService;
            _usersVoteService = usersVoteService;
            _answerService = answerService;
            _voteService = voteService;
            _pluginService = pluginService;
            _cache = cache;
            _logger = logger;
        }

        public IViewComponentResult Invoke()
        {
            if (!_cache.TryGetValue(PluginCacheKey, out bool pluginEnabled))
            {
                var pollPlugin = _pluginService.GetByName("Polls");
                pluginEnabled = pollPlugin != null && pollPlugin.IsEnabled;
                _cache.Set(PluginCacheKey, pluginEnabled, CacheDuration);
            }

            if (!pluginEnabled)
            {
                return View("Disabled");
            }

            if (!_cache.TryGetValue(FeaturedPollsCacheKey, out List<digioz.Portal.Bo.Poll>? polls) || polls == null)
            {
                polls = _pollService.GetLatestFeatured(2);
                _cache.Set(FeaturedPollsCacheKey, polls, CacheDuration);
            }

            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var model = new List<PollMenuItemViewModel>();
            foreach (var p in polls)
            {
                // Answers and the rendered chart are shared by all users; only HasVoted is per-user.
                var pollKey = GetPollCacheKey(p.Id);
                if (!_cache.TryGetValue(pollKey, out CachedPollData? data) || data == null)
                {
                    var answers = _answerService.GetByPollId(p.Id);
                    data = new CachedPollData
                    {
                        Answers = answers,
                        ChartBase64 = GenerateResultsChart(p.Id, answers)
                    };
                    _cache.Set(pollKey, data, CacheDuration);
                }

                var hasVoted = !string.IsNullOrEmpty(userId) && _usersVoteService.Exists(p.Id, userId);
                model.Add(new PollMenuItemViewModel
                {
                    Poll = p,
                    Answers = data.Answers,
                    HasVoted = hasVoted,
                    ResultsChartBase64 = data.ChartBase64
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
