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
using digioz.Portal.Web.Models.ViewModels;
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
            var chats = _chatLogic.GetLatestChats();
            chats.Reverse();

            ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Username = User.Identity.Name;

            return View(chats);
        }

        public async Task<JsonResult> Online()
        {
            var lastOnline = DateTime.Now.AddMinutes(-2);
            var dateTimeLastOnline = DateTime.Now.AddMinutes(-1);
            var online = _visitorSessionLogic.GetGeneric(x => x.PageUrl.Contains("/Chat") && x.DateModified > lastOnline).Select(x => x.Username).Distinct().ToList();       

            return Json(online);
        }
    }
}