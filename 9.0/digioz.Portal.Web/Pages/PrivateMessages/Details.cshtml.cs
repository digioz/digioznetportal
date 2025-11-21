using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class DetailsModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public class ThreadMessage
        {
            public int Id { get; set; }
            public string FromId { get; set; } = string.Empty;
            public string ToId { get; set; } = string.Empty;
            public string FromDisplayName { get; set; } = string.Empty;
            public string ToDisplayName { get; set; } = string.Empty;
            public System.DateTime? SentDate { get; set; }
            public string? Subject { get; set; }
            public string? Message { get; set; }
        }

        public PrivateMessage RootMessage { get; set; }
        public List<ThreadMessage> Thread { get; set; } = new();

        [BindProperty]
        public ReplyInput Reply { get; set; } = new();

        public class ReplyInput
        {
            public int? ParentId { get; set; }
            [Required]
            public string Subject { get; set; } = string.Empty;
            [Required]
            public string Message { get; set; } = string.Empty;
        }

        public IActionResult OnGet(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            RootMessage = _pmService.Get(id);
            if (RootMessage == null || (RootMessage.FromId != currentUserId && RootMessage.ToId != currentUserId))
            {
                return NotFound();
            }
            
            if (RootMessage.ToId == currentUserId && !RootMessage.IsRead)
            {
                _pmService.MarkRead(id);
            }

            var rawThread = _pmService.GetThread(id);
            var profileLookup = _profileService.GetAll()
                .Where(p => !string.IsNullOrWhiteSpace(p.UserId) && !string.IsNullOrWhiteSpace(p.DisplayName))
                .ToDictionary(p => p.UserId, p => p.DisplayName);

            Thread = rawThread.Select(m => new ThreadMessage
            {
                Id = m.Id,
                FromId = m.FromId,
                ToId = m.ToId,
                FromDisplayName = profileLookup.GetValueOrDefault(m.FromId, "Unknown User"),
                ToDisplayName = profileLookup.GetValueOrDefault(m.ToId, "Unknown User"),
                SentDate = m.SentDate,
                Subject = m.Subject,
                Message = m.Message
            }).ToList();

            Reply.Subject = "RE: " + (RootMessage.Subject ?? "(No Subject)");
            Reply.Message = $"\n\n--- On {RootMessage.SentDate:g}, {profileLookup.GetValueOrDefault(RootMessage.FromId, "Unknown User")} wrote: ---\n> " + (RootMessage.Message ?? "").Replace("\n", "\n> ");
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var root = _pmService.Get(id);
            if (root == null || (root.FromId != currentUserId && root.ToId != currentUserId))
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return OnGet(id); // reload data
            }

            var sanitizedMessage = Sanitize(Reply.Message);
            var sanitizedSubject = Sanitize(Reply.Subject);

            var reply = new PrivateMessage
            {
                FromId = currentUserId,
                ToId = currentUserId == root.FromId ? root.ToId : root.FromId,
                Subject = sanitizedSubject,
                Message = sanitizedMessage,
                ParentId = id
            };
            _pmService.Add(reply);
            return RedirectToPage("/PrivateMessages/Details", new { id = root.Id });
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // Parse HTML then extract plain text only; remove all tags, scripts, attributes.
            var doc = new HtmlDocument();
            doc.LoadHtml(input);
            var text = doc.DocumentNode.InnerText ?? string.Empty;
            // Collapse excessive whitespace/newlines
            text = Regex.Replace(text, "\\s+", " ").Trim();
            return text;
        }
    }
}
