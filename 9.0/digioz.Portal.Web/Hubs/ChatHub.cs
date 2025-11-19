using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IProfileService _profileService;
        private readonly HtmlEncoder _encoder = HtmlEncoder.Default;

        public ChatHub(IChatService chatService, IProfileService profileService)
        {
            _chatService = chatService;
            _profileService = profileService;
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var userId = Context.UserIdentifier ?? string.Empty;
            
            // Get DisplayName from Profile
            var profile = _profileService.GetAll().FirstOrDefault(p => p.UserId == userId);
            var displayName = profile?.DisplayName ?? Context.User?.Identity?.Name ?? "Anonymous";

            // Encode before persisting to avoid script injection stored in DB
            var safe = _encoder.Encode(message.Trim());
            _chatService.Add(new Chat
            {
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Message = safe
            });

            // Broadcast with DisplayName
            await Clients.All.SendAsync("ReceiveMessage", displayName, message.Trim());
        }
    }
}
