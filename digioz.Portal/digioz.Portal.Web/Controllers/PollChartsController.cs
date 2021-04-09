using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digioz.Portal.Bo;
using digioz.Portal.Bll;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using digioz.Portal.Bll.Interfaces;
using System.Threading.Tasks;
using System.IO;
using digioz.Portal.Bo.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace digioz.Portal.Web.Controllers
{
    public class PollChartsController : Controller 
    {
        private readonly ILogic<Poll> _pollLogic;
        private readonly ILogic<PollAnswer> _pollAnswerLogic;
        private readonly ILogic<PollVote> _pollVoteLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PollChartsController(
            ILogic<Poll> pollLogic,
            ILogic<PollAnswer> pollAnswerLogic,
            ILogic<PollVote> pollVoteLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _pollLogic = pollLogic;
            _pollAnswerLogic = pollAnswerLogic;
            _pollVoteLogic = pollVoteLogic;
            _webHostEnvironment = webHostEnvironment;
        }

        private string GetImagePath()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        // GET: Poll
        public ActionResult Index(string id)
        {
            Guid idGuid;
            bool isIdAGUID = Guid.TryParse(id, out idGuid);

            if (string.IsNullOrEmpty(id) || isIdAGUID == false)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {

                var poll = _pollLogic.Get(id);
                var chartLogic = new ChartLogic();
                var pollResults = chartLogic.GetPollResults(id);

                ViewBag.PollTitle = poll.Slug;
                ViewBag.PollColumn = "Answer";

                return View(pollResults);
            }
            catch 
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}