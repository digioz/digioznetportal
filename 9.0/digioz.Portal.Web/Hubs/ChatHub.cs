using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Concurrent;
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
        
        // Static dictionary to track connected users: ConnectionId -> (UserId, DisplayName)
        private static readonly ConcurrentDictionary<string, (string UserId, string DisplayName)> ConnectedUsers 
            = new ConcurrentDictionary<string, (string UserId, string DisplayName)>();

        public ChatHub(IChatService chatService, IProfileService profileService)
        {
            _chatService = chatService;
            _profileService = profileService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? string.Empty;
            var profile = _profileService.GetAll().FirstOrDefault(p => p.UserId == userId);
            var displayName = profile?.DisplayName ?? Context.User?.Identity?.Name ?? "Anonymous";
            
            // Add user to connected users
            ConnectedUsers.TryAdd(Context.ConnectionId, (userId, displayName));
            
            // Notify all clients about the updated user list
            await Clients.All.SendAsync("UpdateUserList", GetUniqueUsers());
            
            // Notify that user joined
            await Clients.Others.SendAsync("UserJoined", displayName);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove user from connected users
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
            {
                // Notify all clients about the updated user list
                await Clients.All.SendAsync("UpdateUserList", GetUniqueUsers());
                
                // Notify that user left
                await Clients.Others.SendAsync("UserLeft", userInfo.DisplayName);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            
            // Enforce maximum message length
            const int maxMessageLength = 1000;
            if (message.Length > maxMessageLength)
            {
                message = message.Substring(0, maxMessageLength);
            }
            
            var userId = Context.UserIdentifier ?? string.Empty;
            
            // Get DisplayName from Profile
            var profile = _profileService.GetAll().FirstOrDefault(p => p.UserId == userId);
            var displayName = profile?.DisplayName ?? Context.User?.Identity?.Name ?? "Anonymous";

            // Properly sanitize and encode input to prevent XSS attacks
            // HTML encode the message before storing to prevent stored XSS
            var trimmedMessage = message.Trim();
            var sanitizedMessage = HttpUtility.HtmlEncode(trimmedMessage);
            
            _chatService.Add(new Chat
            {
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Message = sanitizedMessage  // Store HTML-encoded version
            });

            // Broadcast raw trimmed message (not encoded) - client will encode it for display
            // This prevents double-encoding for real-time messages while keeping stored messages safe
            await Clients.All.SendAsync("ReceiveMessage", displayName, trimmedMessage);
        }
        
        // Helper method to get unique users (removes duplicate connections from same user)
        private static string[] GetUniqueUsers()
        {
            return ConnectedUsers.Values
                .GroupBy(u => u.UserId)
                .Select(g => g.First().DisplayName)
                .OrderBy(name => name)
                .ToArray();
        }
    }
}
