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
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 5;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public string CurrentUserId { get; private set; } = string.Empty;
        public HashSet<string> UserVotedPollIds { get; private set; } = new();

        public void OnGet()
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            var allPolls = _pollService.GetAll();
            
            var filteredPolls = allPolls.Where(p =>
            {
                bool isOwnPoll = !string.IsNullOrEmpty(CurrentUserId) && p.UserId == CurrentUserId;
                bool isVisibleAndApproved = p.Visible == true && p.Approved == true;
                
                return isOwnPoll || isVisibleAndApproved;
            })
            .OrderByDescending(p => p.DateCreated)
            .ToList();

            TotalCount = filteredPolls.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 5;

            var skip = (PageNumber - 1) * PageSize;
            Items = filteredPolls.Skip(skip).Take(PageSize).ToList();

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                var userVotes = _usersVoteService.GetByUserId(CurrentUserId);
                UserVotedPollIds = new HashSet<string>(userVotes.Select(v => v.PollId));
            }
        }
    }
}