using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using System;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public AddModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public class UserLite { public string Id { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string ToDisplayName { get; set; } = string.Empty; // user types this (Profile.DisplayName)
            public string ToId { get; set; } = string.Empty; // resolved to Identity UserId
            [Required]
            public string Subject { get; set; } = string.Empty;
            [Required]
            public string Message { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var typed = (Input.ToDisplayName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(typed))
            {
                var users = _profileService.GetAll()
                    .Where(p => !string.IsNullOrWhiteSpace(p.DisplayName))
                    .Select(p => new UserLite { Id = p.UserId, DisplayName = p.DisplayName })
                    .ToList();

                // First try exact (case-insensitive), then partial contains
                var exact = users.Where(u => string.Equals(u.DisplayName, typed, StringComparison.OrdinalIgnoreCase)).ToList();
                if (exact.Count == 1)
                {
                    Input.ToId = exact[0].Id;
                }
                else if (exact.Count > 1)
                {
                    ModelState.AddModelError(nameof(Input.ToDisplayName), $"Multiple users match '{typed}'. Please refine.");
                }
            }

            var currentUserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Input.ToId))
            {
                if (string.IsNullOrWhiteSpace(Input.ToId))
                {
                    ModelState.AddModelError(nameof(Input.ToDisplayName), "Recipient not found.");
                }
                return Page();
            }
            if (Input.ToId == currentUserId)
            {
                ModelState.AddModelError(nameof(Input.ToDisplayName), "You cannot send a private message to yourself.");
                return Page();
            }

            var sanitizedMessage = Sanitize(Input.Message);
            var pm = new PrivateMessage
            {
                FromId = currentUserId,
                ToId = Input.ToId,
                Subject = Input.Subject,
                Message = sanitizedMessage
            };
            _pmService.Add(pm);
            return RedirectToPage("/PrivateMessages/Details", new { id = pm.Id });
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
