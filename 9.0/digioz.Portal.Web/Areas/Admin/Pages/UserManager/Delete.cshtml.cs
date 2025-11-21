using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Authentication;

namespace digioz.Portal.Web.Areas.Admin.Pages.UserManager
{
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly IAspNetUserService _userService;
        private readonly IProfileService _profileService;
        private readonly IChatService _chatService;
        private readonly ICommentService _commentService;
        private readonly IPollService _pollService;
        private readonly IVideoService _videoService;
        private readonly IPictureService _pictureService;
        private readonly IOrderService _orderService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            IAspNetUserService userService,
            IProfileService profileService,
            IChatService chatService,
            ICommentService commentService,
            IPollService pollService,
            IVideoService videoService,
            IPictureService pictureService,
            IOrderService orderService,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<DeleteModel> logger)
        {
            _userService = userService;
            _profileService = profileService;
            _chatService = chatService;
            _commentService = commentService;
            _pollService = pollService;
            _videoService = videoService;
            _pictureService = pictureService;
            _orderService = orderService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public new AspNetUser? User { get; set; }
        public Profile? Profile { get; set; }
        public UserRelatedData? RelatedData { get; set; }

        [BindProperty]
        public DeleteOptions Options { get; set; } = new();

        public string? StatusMessage { get; set; }

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

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User = _userService.Get(id);
            if (User == null)
            {
                return NotFound();
            }

            Profile = _profileService.GetByUserId(id);

            // Use efficient count methods instead of loading all records into memory
            RelatedData = new UserRelatedData
            {
                PictureCount = _pictureService.CountByUserId(id),
                VideoCount = _videoService.CountByUserId(id),
                PollCount = _pollService.CountByUserId(id),
                ChatCount = _chatService.CountByUserId(id),
                CommentCount = _commentService.CountByUserId(id),
                OrderCount = _orderService.CountByUserId(id)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User = _userService.Get(id);
            if (User == null)
            {
                return NotFound();
            }

            // Check if admin is trying to delete themselves (using base.User which is ClaimsPrincipal)
            var currentUserId = _userManager.GetUserId(base.User);
            if (currentUserId == id)
            {
                StatusMessage = "Error: You cannot delete your own account while logged in.";
                await LoadRelatedDataAsync(id);
                return Page();
            }

            try
            {
                // Get the System user ID for reassignment
                var systemProfile = _profileService.GetAll()
                    .FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Equals("System", StringComparison.OrdinalIgnoreCase));
                
                string? systemUserId = systemProfile?.UserId;

                if (string.IsNullOrEmpty(systemUserId))
                {
                    StatusMessage = "Error: System user not found. Please ensure a user with DisplayName 'System' exists before deleting users.";
                    await LoadRelatedDataAsync(id);
                    return Page();
                }

                // Handle Pictures - Delete or Reassign (using efficient filtered query)
                var pictures = _pictureService.GetFiltered(userId: id, isAdmin: true);
                if (pictures.Any())
                {
                    if (Options.DeletePictures)
                    {
                        foreach (var picture in pictures)
                        {
                            _pictureService.Delete(picture.Id);
                        }
                    }
                    else
                    {
                        // Reassign to System user
                        foreach (var picture in pictures)
                        {
                            picture.UserId = systemUserId;
                            _pictureService.Update(picture);
                        }
                    }
                }

                // Handle Videos - Delete or Reassign (using efficient filtered query)
                var videos = _videoService.GetFiltered(userId: id, isAdmin: true);
                if (videos.Any())
                {
                    if (Options.DeleteVideos)
                    {
                        foreach (var video in videos)
                        {
                            _videoService.Delete(video.Id);
                        }
                    }
                    else
                    {
                        // Reassign to System user
                        foreach (var video in videos)
                        {
                            video.UserId = systemUserId;
                            _videoService.Update(video);
                        }
                    }
                }

                // Handle Polls - Delete or Reassign (using efficient filtered query)
                var polls = _pollService.GetByUserId(id);
                if (polls.Any())
                {
                    if (Options.DeletePolls)
                    {
                        foreach (var poll in polls)
                        {
                            _pollService.Delete(poll.Id);
                        }
                    }
                    else
                    {
                        // Reassign to System user
                        foreach (var poll in polls)
                        {
                            poll.UserId = systemUserId;
                            _pollService.Update(poll);
                        }
                    }
                }

                // Handle Chat Messages - Delete or Reassign (using efficient filtered query)
                var chats = _chatService.GetByUserId(id);
                if (chats.Any())
                {
                    if (Options.DeleteChat)
                    {
                        foreach (var chat in chats)
                        {
                            _chatService.Delete(chat.Id);
                        }
                    }
                    else
                    {
                        // Reassign to System user
                        foreach (var chat in chats)
                        {
                            chat.UserId = systemUserId;
                            _chatService.Update(chat);
                        }
                    }
                }

                // Handle Comments - Delete or Reassign (using efficient filtered query)
                var comments = _commentService.GetByUserId(id);
                if (comments.Any())
                {
                    if (Options.DeleteComments)
                    {
                        foreach (var comment in comments)
                        {
                            _commentService.Delete(comment.Id);
                        }
                    }
                    else
                    {
                        // Reassign to System user
                        foreach (var comment in comments)
                        {
                            comment.UserId = systemUserId;
                            _commentService.Update(comment);
                        }
                    }
                }

                // Handle Orders - Delete or Reassign (using efficient filtered query)
                var orders = _orderService.GetByUserId(id);
                if (orders.Any())
                {
                    if (Options.DeleteOrders)
                    {
                        foreach (var order in orders)
                        {
                            _orderService.Delete(order.Id);
                        }
                    }
                    else
                    {
                        // Reassign to System user
                        foreach (var order in orders)
                        {
                            order.UserId = systemUserId;
                            _orderService.Update(order);
                        }
                    }
                }

                // Delete profile
                var profile = _profileService.GetByUserId(id);
                if (profile != null)
                {
                    _profileService.Delete(profile.Id);
                }

                // Get the user's identity to invalidate their session
                var identityUser = await _userManager.FindByIdAsync(id);
                if (identityUser != null)
                {
                    // Remove from all roles first
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    if (roles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(identityUser, roles);
                    }

                    // CRITICAL: Update security stamp to invalidate all existing tokens/sessions
                    // This forces the user to be signed out on their next request
                    await _userManager.UpdateSecurityStampAsync(identityUser);
                    
                    _logger.LogInformation("Security stamp updated for user {UserId}. User will be signed out on next request.", id);

                    // Delete the user
                    var deleteResult = await _userManager.DeleteAsync(identityUser);
                    
                    if (deleteResult.Succeeded)
                    {
                        _logger.LogInformation("User {UserId} ({UserName}) successfully deleted by {AdminUser}", 
                            id, identityUser.UserName, base.User.Identity?.Name);
                    }
                    else
                    {
                        _logger.LogError("Failed to delete user {UserId}: {Errors}", 
                            id, string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                        
                        StatusMessage = $"Error deleting user: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}";
                        await LoadRelatedDataAsync(id);
                        return Page();
                    }
                }

                StatusMessage = "User deleted successfully. The user's security stamp has been invalidated and they will be forcefully signed out on their next request.";
                return RedirectToPage("/UserManager/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                StatusMessage = $"Error deleting user: {ex.Message}";
                await LoadRelatedDataAsync(id);
                return Page();
            }
        }

        private async Task LoadRelatedDataAsync(string userId)
        {
            User = _userService.Get(userId);
            if (User != null)
            {
                Profile = _profileService.GetByUserId(userId);
                
                // Use efficient count methods instead of loading all records into memory
                RelatedData = new UserRelatedData
                {
                    PictureCount = _pictureService.CountByUserId(userId),
                    VideoCount = _videoService.CountByUserId(userId),
                    PollCount = _pollService.CountByUserId(userId),
                    ChatCount = _chatService.CountByUserId(userId),
                    CommentCount = _commentService.CountByUserId(userId),
                    OrderCount = _orderService.CountByUserId(userId)
                };
            }
        }
    }
}
