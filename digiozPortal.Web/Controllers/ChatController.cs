using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using digiozPortal.BLL;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Controllers
{
    public class ChatController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        ILogic<Chat> _chatLogic;
        ILogic<Config> _configLogic;
        ILogic<VisitorSession> _visitorSessionLogic;

        public ChatController(
            ILogger<HomeController> logger,
            ILogic<Chat> pageLogic,
            ILogic<Config> configLogic,
            ILogic<VisitorSession> visitorSessionLogic
        ) {
            _logger = logger;
            _chatLogic = pageLogic;
            _configLogic = configLogic;
            _visitorSessionLogic = visitorSessionLogic;
        }

        // GET: Chat
        [Authorize]
        public ActionResult Index()
        {
            var chats = new List<Chat>(); // _chatLogic.GetLatestChats();

            return View(chats);
        }

        [Authorize]
        [HttpPost]
        public JsonResult Add(string message)
        {
            if (!String.IsNullOrEmpty(message) && message != "[object HTMLInputElement]")
            {
                Chat chat = new Chat
                {
                    Timestamp = DateTime.Now,
                    Message = HttpUtility.HtmlEncode(message),
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                };

                _chatLogic.Add(chat);
            }

            return null;
        }

        public JsonResult Online()
        {
            var dateTimeLastOnline = DateTime.Now.AddMinutes(-1);
            var online = new List<VisitorSession>(); // _visitorSessionLogic.GetChatVisitorSessions(dateTimeLastOnline);       

            return Json(online);
        }
    }
}