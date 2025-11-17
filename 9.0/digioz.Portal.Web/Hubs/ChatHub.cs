using System;
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
        private readonly HtmlEncoder _encoder = HtmlEncoder.Default;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var userId = Context.UserIdentifier ?? string.Empty;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            // Encode before persisting to avoid script injection stored in DB
            var safe = _encoder.Encode(message.Trim());
            _chatService.Add(new Chat
            {
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                Message = safe
            });

            // Broadcast raw (unencoded) message; clients are expected to encode
            await Clients.All.SendAsync("ReceiveMessage", userName, message.Trim());
        }
    }
}
