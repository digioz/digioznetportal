using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Controllers
{
    public class ChatController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IChatLogic _chatLogic;
        private readonly ILogic<VisitorSession> _visitorSessionLogic;

        public ChatController(
            ILogger<HomeController> logger,
            IChatLogic chatLogic,
            ILogic<VisitorSession> visitorSessionLogic
        ) {
            _logger = logger;
            _chatLogic = chatLogic;
            _visitorSessionLogic = visitorSessionLogic;
        }

        // GET: Chat
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var query = @"SELECT TOP (50) * FROM [Chat] ORDER BY [Id] DESC;";
            var chats = (List<Chat>)_chatLogic.GetQueryString(query);
            chats.Reverse();

            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Username = User.Identity.Name;

            return View(chats);
        }

        public async Task<JsonResult> Online()
        {
            var dateTimeLastOnline = DateTime.Now.AddMinutes(-1);
            var online = new List<VisitorSession>(); // _visitorSessionLogic.GetChatVisitorSessions(dateTimeLastOnline);       

            return Json(online);
        }
    }
}