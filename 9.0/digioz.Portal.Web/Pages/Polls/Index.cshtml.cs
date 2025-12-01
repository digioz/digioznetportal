using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Pages.Polls
{
    public class IndexModel : PageModel
    {
        private readonly IPollService _pollService;
        private readonly IPollUsersVoteService _usersVoteService;
        public IndexModel(IPollService pollService, IPollUsersVoteService usersVoteService)
        {
            _pollService = pollService;
            _usersVoteService = usersVoteService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Poll> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Poll>();
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 5;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, pageSize));
        public string CurrentUserId { get; private set; } = string.Empty;
        public HashSet<string> UserVotedPollIds { get; private set; } = new();

        public void OnGet()
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var all = _pollService.GetAll().OrderByDescending(p => p.DateCreated).ToList();
            TotalCount = all.Count;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 5;
            var skip = (pageNumber - 1) * pageSize;
            Items = all.Skip(skip).Take(pageSize).ToList();

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                var voted = _usersVoteService.GetAll().Where(v => v.UserId == CurrentUserId).Select(v => v.PollId).Distinct();
                UserVotedPollIds = new HashSet<string>(voted);
            }
        }
    }
}