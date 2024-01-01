using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using digioz.Portal.Bo;
using digioz.Portal.Dal;
using Microsoft.AspNetCore.Authorization;
using digioz.Portal.Bll.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using digioz.Portal.Web.Models.ViewModels;
using System.IO;
using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ChatManagerController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IChatLogic _chatLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChatManagerController(
            ILogger<HomeController> logger,
            IChatLogic chatLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _logger = logger;
            _chatLogic = chatLogic;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetAppDataFolderPath()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "data");

            return path;
        }

        public ActionResult Index()
        {
            var query = "SELECT TOP (1000) * FROM [Chat] ORDER BY ID DESC;";
            var chats = _chatLogic.GetQueryString(query).ToList();
            
            return View(chats);
        }

        public ActionResult Delete(int id)
        {
            var chat = _chatLogic.Get(id);

            if (chat != null)
			{
                _chatLogic.Delete(chat);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Purge()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Purge(ChatExportPurgeViewModel purgeRange)
        {
            List<Chat> chats = null;

            if (purgeRange.ProcessAll)
            {
                chats = _chatLogic.GetAll();
            }
            else
            {
                if (!String.IsNullOrEmpty(purgeRange.StartDate) && !String.IsNullOrEmpty(purgeRange.EndDate))
                {
                    DateTime startDate = DateTime.ParseExact(purgeRange.StartDate, "MM/dd/yyyy", null);
                    DateTime endDate = DateTime.ParseExact(purgeRange.EndDate, "MM/dd/yyyy", null);

                    chats = _chatLogic.GetGeneric(x => x.Timestamp <= endDate && x.Timestamp >= startDate).ToList();
                }
            }

            if (chats != null && chats.Count > 0)
            {
                foreach(var chat in chats)
				{
                    _chatLogic.Delete(chat);
				}

                ViewBag.Status = "Success";
                ViewBag.Message = "<strong>Success!</strong> You successfully purged the chat messages.";
            }
            else
            {
                ViewBag.Status = "Failed";
                ViewBag.Message = "<strong>Oh snap!</strong> We were not able to purge the chat messages. Please make sure to either select a date range or check the \"Export All\" checkbox.";
            }

            return View();
        }

        public ActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Export(ChatExportPurgeViewModel exportRange)
        {
            List<Chat> chats = null;
            string fileName = "chats_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".csv";
            string path = GetAppDataFolderPath() + "\\" + fileName;

            if (exportRange.ProcessAll)
            {
                chats = _chatLogic.GetAll();
            }
            else
            {
                if (!String.IsNullOrEmpty(exportRange.StartDate) && !String.IsNullOrEmpty(exportRange.EndDate))
                {
                    DateTime startDate = DateTime.ParseExact(exportRange.StartDate, "MM/dd/yyyy", null);
                    DateTime endDate = DateTime.ParseExact(exportRange.EndDate, "MM/dd/yyyy", null);

                    chats = _chatLogic.GetGeneric(x => x.Timestamp <= endDate && x.Timestamp >= startDate);
                }
            }

            if (chats != null && chats.Count > 0)
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Id,UserId,Timestamp,Message");

                    foreach (var chat in chats)
                    {
                        stringBuilder.AppendLine($"{chat.Id},{ chat.UserId},{ chat.Timestamp }, { chat.Message }");
                    }

                    return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", fileName);
                }
                catch
                {
                    ViewBag.Status = "Failed";
                    ViewBag.Message = "<strong>Oh snap!</strong> We were not able to export the chat messages. Please make sure to either select a date range or check the \"Export All\" checkbox.";
                }               
            }
            else
            {
                ViewBag.Status = "Failed";
                ViewBag.Message = "<strong>Oh snap!</strong> We were not able to export the chat messages. Please make sure to either select a date range or check the \"Export All\" checkbox.";
            }

            return View();
        }
    }
}