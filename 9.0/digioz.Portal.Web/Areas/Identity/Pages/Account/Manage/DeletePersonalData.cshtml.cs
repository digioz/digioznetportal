// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;
        private readonly IPollService _pollService;
        private readonly IChatService _chatService;
        private readonly ICommentService _commentService;
        private readonly IOrderService _orderService;
        private readonly IProfileService _profileService;

        public DeletePersonalDataModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IPictureService pictureService,
            IVideoService videoService,
            IPollService pollService,
            IChatService chatService,
            ICommentService commentService,
            IOrderService orderService,
            IProfileService profileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _pictureService = pictureService;
            _videoService = videoService;
            _pollService = pollService;
            _chatService = chatService;
            _commentService = commentService;
            _orderService = orderService;
            _profileService = profileService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        [BindProperty]
        public DeleteOptions Options { get; set; } = new();

        public class DeleteOptions
        {
            public bool DeletePictures { get; set; }
            public bool DeleteVideos { get; set; }
            public bool DeletePolls { get; set; }
            public bool DeleteChat { get; set; }
            public bool DeleteComments { get; set; }
            public bool DeleteOrders { get; set; }
        }

        public class UserRelatedData
        {
            public int PictureCount { get; set; }
            public int VideoCount { get; set; }
            public int PollCount { get; set; }
            public int ChatCount { get; set; }
            public int CommentCount { get; set; }
            public int OrderCount { get; set; }
        }

        public bool RequirePassword { get; set; }
        public UserRelatedData RelatedData { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);

            // Get user content counts
            RelatedData = new UserRelatedData
            {
                PictureCount = _pictureService.CountByUserId(user.Id),
                VideoCount = _videoService.CountByUserId(user.Id),
                PollCount = _pollService.CountByUserId(user.Id),
                ChatCount = _chatService.CountByUserId(user.Id),
                CommentCount = _commentService.CountByUserId(user.Id),
                OrderCount = _orderService.CountByUserId(user.Id)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    
                    // Reload related data
                    RelatedData = new UserRelatedData
                    {
                        PictureCount = _pictureService.CountByUserId(user.Id),
                        VideoCount = _videoService.CountByUserId(user.Id),
                        PollCount = _pollService.CountByUserId(user.Id),
                        ChatCount = _chatService.CountByUserId(user.Id),
                        CommentCount = _commentService.CountByUserId(user.Id),
                        OrderCount = _orderService.CountByUserId(user.Id)
                    };
                    
                    return Page();
                }
            }

            var userId = user.Id;

            try
            {
                // Get the System user ID for reassignment
                var systemProfile = _profileService.GetAll()
                    .FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Equals("System", StringComparison.OrdinalIgnoreCase));
                
                string systemUserId = systemProfile?.UserId;

                if (string.IsNullOrEmpty(systemUserId))
                {
                    _logger.LogWarning("System user not found. Content will be deleted instead of reassigned.");
                }

                // Handle Pictures - Delete or Reassign
                if (Options.DeletePictures)
                {
                    _pictureService.DeleteByUserId(userId);
                }
                else if (!string.IsNullOrEmpty(systemUserId))
                {
                    _pictureService.ReassignByUserId(userId, systemUserId);
                }

                // Handle Videos - Delete or Reassign
                if (Options.DeleteVideos)
                {
                    _videoService.DeleteByUserId(userId);
                }
                else if (!string.IsNullOrEmpty(systemUserId))
                {
                    _videoService.ReassignByUserId(userId, systemUserId);
                }

                // Handle Polls - Delete or Reassign
                if (Options.DeletePolls)
                {
                    _pollService.DeleteByUserId(userId);
                }
                else if (!string.IsNullOrEmpty(systemUserId))
                {
                    _pollService.ReassignByUserId(userId, systemUserId);
                }

                // Handle Chat Messages - Delete or Reassign
                if (Options.DeleteChat)
                {
                    _chatService.DeleteByUserId(userId);
                }
                else if (!string.IsNullOrEmpty(systemUserId))
                {
                    _chatService.ReassignByUserId(userId, systemUserId);
                }

                // Handle Comments - Delete or Reassign
                if (Options.DeleteComments)
                {
                    _commentService.DeleteByUserId(userId);
                }
                else if (!string.IsNullOrEmpty(systemUserId))
                {
                    _commentService.ReassignByUserId(userId, systemUserId);
                }

                // Handle Orders - Delete or Reassign
                if (Options.DeleteOrders)
                {
                    _orderService.DeleteByUserId(userId);
                }
                else if (!string.IsNullOrEmpty(systemUserId))
                {
                    _orderService.ReassignByUserId(userId, systemUserId);
                }

                // Delete profile
                var profile = _profileService.GetByUserId(userId);
                if (profile != null)
                {
                    _profileService.Delete(profile.Id);
                }

                // Delete the user account
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user.");
                }

                await _signInManager.SignOutAsync();

                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

                return Redirect("~/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                ModelState.AddModelError(string.Empty, $"An error occurred while deleting your account: {ex.Message}");
                
                // Reload related data
                RelatedData = new UserRelatedData
                {
                    PictureCount = _pictureService.CountByUserId(userId),
                    VideoCount = _videoService.CountByUserId(userId),
                    PollCount = _pollService.CountByUserId(userId),
                    ChatCount = _chatService.CountByUserId(userId),
                    CommentCount = _commentService.CountByUserId(userId),
                    OrderCount = _orderService.CountByUserId(userId)
                };
                
                return Page();
            }
        }
    }
}
