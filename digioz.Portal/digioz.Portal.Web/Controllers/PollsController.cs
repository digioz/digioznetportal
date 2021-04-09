using digioz.Portal.Web.Models.ViewModels;
using System;
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
using Microsoft.AspNetCore.Http;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;

namespace digioz.Portal.Web.Controllers
{
    public class PollsController : Controller
    {
        private readonly ILogic<Poll> _pollLogic;
        private readonly ILogic<PollAnswer> _pollAnswerLogic;
        private readonly ILogic<PollVote> _pollVoteLogic;
        private readonly ILogic<PollUsersVote> _pollUsersVoteLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;

        public PollsController(
            ILogic<Poll> pollLogic,
            ILogic<PollAnswer> pollAnswerLogic,
            ILogic<PollVote> pollVoteLogic,
            ILogic<PollUsersVote> pollUsersVoteLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic
        )
        {
            _pollLogic = pollLogic;
            _pollAnswerLogic = pollAnswerLogic;
            _pollVoteLogic = pollVoteLogic;
            _pollUsersVoteLogic = pollUsersVoteLogic;
            _configLogic = configLogic;
            _userLogic = userLogic;
        }

        // GET: Poll
        public async Task<IActionResult> Index()
        {
            var polls = _pollLogic.GetAll();
            return View(polls);
        }

        [Authorize]
        public async Task<IActionResult> Add(string id)
        {
            Guid idGuid;
            bool isIdAGUID = Guid.TryParse(id, out idGuid);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(id) || isIdAGUID == false)
            {
                return RedirectToAction("Index", "Home");
            }

            Poll poll = _pollLogic.Get(id);
            var pollUsersVote = _pollUsersVoteLogic.GetGeneric(x => x.PollId == poll.Id && x.UserId == userId);

            if (pollUsersVote != null && pollUsersVote.Count > 0)
			{
                ViewBag.HasUserVoted = true;
            }
            else
			{
                ViewBag.HasUserVoted = false;
            }
            
            ViewBag.PollAnswers = _pollAnswerLogic.GetGeneric(x => x.PollId == poll.Id);

            Response.ContentType = "text/html";

            return View(poll);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(IFormCollection form)
        {
            string id = form["Id"].ToString();
            Poll poll = _pollLogic.Get(id);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userLogic.Get(userId);

            Guid idGuid;
            bool isIdAGUID = Guid.TryParse(id, out idGuid);

            if (string.IsNullOrEmpty(id) || isIdAGUID == false)
            {
                return RedirectToAction("Index", "Home");
            }

            if (poll.AllowMultipleOptionsVote)
            {
                var pollAnswers = new List<PollAnswer>();

                foreach (string key in form.Keys)
                {
                    if (key.Contains("answerid_"))
                    {
                        string formValue = form[key].ToString().Trim();
                        string keyId = key.ToString().Split('_')[1].ToString().Trim();

                        if (formValue == "on")
                        {
                            PollVote pollVote = new PollVote
                            {
                                Id = Guid.NewGuid().ToString(),
                                PollAnswerId = _pollAnswerLogic.Get(keyId).Id,
                                UserId = user.Id
                            };

                            _pollVoteLogic.Add(pollVote);
                        }
                    }
                }
            }
            else
            {
                string keyId = form["answer"].ToString().Trim();

                PollVote pollVote = new PollVote
                {
                    PollAnswerId = _pollAnswerLogic.Get(keyId).Id,
                    UserId = user.Id
                };

                _pollVoteLogic.Add(pollVote);
            }

            // Record who the user was that just voted
            var pollUsersVote = new PollUsersVote
            {
                UserId = userId,
                PollId = id,
                DateVoted = DateTime.Now.ToString()
            };

            _pollUsersVoteLogic.Add(pollUsersVote);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var poll = _pollLogic.Get(id);

            if (poll == null)
            {
                return BadRequest();
            }

            return View(poll);
        }
    }
}