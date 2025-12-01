using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.Shared.Components.PollMenu
{
    public class PollMenuViewComponent : ViewComponent
    {
        private readonly IPollService _pollService;
        private readonly IPollUsersVoteService _usersVoteService;
        private readonly IPollAnswerService _answerService;
        private readonly IPluginService _pluginService;

        public PollMenuViewComponent(IPollService pollService, IPollUsersVoteService usersVoteService, IPollAnswerService answerService, IPluginService pluginService)
        {
            _pollService = pollService;
            _usersVoteService = usersVoteService;
            _answerService = answerService;
            _pluginService = pluginService;
        }

        public IViewComponentResult Invoke()
        {
            var pollPlugin = _pluginService.GetAll().FirstOrDefault(p => p.Name == "Poll" && p.IsEnabled);
            if (pollPlugin == null)
            {
                return View("Disabled");
            }

            var polls = _pollService.GetAll().OrderByDescending(p => p.DateCreated).Take(2).ToList();
            var userId = HttpContext.User.FindFirstValue(ClaimsPrincipal.Current != null ? ClaimTypes.NameIdentifier : ClaimTypes.NameIdentifier) ?? HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var votedPolls = new HashSet<string>();
            if (!string.IsNullOrEmpty(userId))
            {
                votedPolls = _usersVoteService.GetAll().Where(v => v.UserId == userId).Select(v => v.PollId).ToHashSet();
            }

            var model = polls.Select(p => new PollMenuItem
            {
                Poll = p,
                Answers = _answerService.GetAll().Where(a => a.PollId == p.Id).ToList(),
                HasVoted = votedPolls.Contains(p.Id)
            }).ToList();

            return View(model);
        }

        public class PollMenuItem
        {
            public digioz.Portal.Bo.Poll Poll { get; set; } = new();
            public List<digioz.Portal.Bo.PollAnswer> Answers { get; set; } = new();
            public bool HasVoted { get; set; }
        }
    }
}
