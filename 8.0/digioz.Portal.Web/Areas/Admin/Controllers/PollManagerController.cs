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

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class PollManagerController : Controller
    {
        private readonly ILogic<Poll> _pollLogic;
        private readonly ILogic<PollAnswer> _pollAnswerLogic;
        private readonly ILogic<PollVote> _pollVoteLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<AspNetUser> _userLogic;

        public PollManagerController(
            ILogic<Poll> pollLogic,
            ILogic<PollAnswer> pollAnswerLogic,
            ILogic<PollVote> pollVoteLogic,
            IConfigLogic configLogic,
            ILogic<AspNetUser> userLogic
        )
        {
            _pollLogic = pollLogic;
            _pollAnswerLogic = pollAnswerLogic;
            _pollVoteLogic = pollVoteLogic;
            _configLogic = configLogic;
            _userLogic = userLogic;
        }

        public async Task<IActionResult> Index()
        {
            var polls = _pollLogic.GetAll();
            return View(polls);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var poll = new Poll();
            var pollAnswers = new List<PollAnswer>();

			poll.Id = Guid.NewGuid().ToString();
            poll.Slug = form["Slug"].ToString().Trim();
            poll.IsClosed = Convert.ToBoolean(form["IsClosed"].ToString().Split(',')[0].Trim());
            poll.DateCreated = DateTime.Now;
            poll.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            poll.Featured = Convert.ToBoolean(form["Featured"].ToString().Split(',')[0].Trim());
            poll.AllowMultipleOptionsVote = Convert.ToBoolean(form["AllowMultipleOptionsVote"].ToString().Split(',')[0].Trim());

            if (!string.IsNullOrEmpty(poll.Slug) && poll.UserId != null)
            {
                _pollLogic.Add(poll);

                foreach (string key in form.Keys)
                {
                    if (key.Contains("pollanswer"))
                    {
                        string formValue = form[key].ToString().Trim();

                        if (!string.IsNullOrEmpty(formValue))
                        {
                            PollAnswer pollAnswer = new PollAnswer();
                            pollAnswer.Id = Guid.NewGuid().ToString();
                            pollAnswer.Answer = formValue;
                            pollAnswer.PollId = poll.Id;

                            pollAnswers.Add(pollAnswer);
                        }
                    }
                }

                foreach(var pollAnswer in pollAnswers)
				{
                    _pollAnswerLogic.Add(pollAnswer);
				}
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string id)
        {
            Poll poll = _pollLogic.Get(id);
            ViewBag.PollAnswers = _pollAnswerLogic.GetGeneric(x => x.PollId == id);

            return View(poll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Poll poll)
        {
            Poll pollExisting = _pollLogic.Get(poll.Id.ToString());
            pollExisting.Slug = poll.Slug;
            pollExisting.IsClosed = poll.IsClosed;
            pollExisting.Featured = poll.Featured;
            pollExisting.AllowMultipleOptionsVote = poll.AllowMultipleOptionsVote;

            if (ModelState.IsValid)
            {
                _pollLogic.Edit(pollExisting);

                return RedirectToAction("Index");
            }

            return View(poll);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            PollViewModel pollViewModel = new PollViewModel();
            pollViewModel.Poll = _pollLogic.Get(id);
            pollViewModel.PollAnswers = _pollAnswerLogic.GetGeneric(x => x.PollId == id);
            
            if (pollViewModel.Poll == null)
            {
                return NotFound();
            }

            return View(pollViewModel);
        }

        public async Task<IActionResult> DeleteAnswer(string id)
        {
            var pollAnswer = _pollAnswerLogic.Get(id);
            string pollId = pollAnswer.PollId;

            if (pollAnswer != null)
            {
                var pollVotes = _pollVoteLogic.GetGeneric(x => x.PollAnswerId == pollAnswer.Id);

                foreach (var pollVote in pollVotes)
				{
                    _pollVoteLogic.Delete(pollVote);
				}
            }

            _pollAnswerLogic.Delete(pollAnswer);

            return RedirectToAction("Details", new { id = pollId });
        }

        public async Task<IActionResult> CreateAnswer(string id)
        {
            var poll = _pollLogic.Get(id);
            PollAnswer pollAnswer = new PollAnswer();
            pollAnswer.PollId = poll.Id;

            return View(pollAnswer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnswer([Bind("PollId, Answer")] PollAnswer pollAnswer)
        {
            var poll = _pollLogic.Get(pollAnswer.PollId);
            pollAnswer.Id = Guid.NewGuid().ToString();
            pollAnswer.PollId = poll.Id;

            if (ModelState.IsValid)
            {
                _pollAnswerLogic.Add(pollAnswer);
            }

            return RedirectToAction("Details", new { id = pollAnswer.PollId });
        }

        public async Task<IActionResult> EditAnswer(string id)
        {
            PollAnswer pollAnswer = _pollAnswerLogic.Get(id);

            return View(pollAnswer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnswer(PollAnswer pollAnswer)
        {
            PollAnswer pollAnswerExisting = _pollAnswerLogic.Get(pollAnswer.Id.ToString());

            if (pollAnswerExisting != null)
            {
                pollAnswerExisting.Answer = pollAnswer.Answer;

                if (ModelState.IsValid)
                {
                    _pollAnswerLogic.Edit(pollAnswerExisting);
                }

                return RedirectToAction("Details", new { id = pollAnswerExisting.PollId });
            }

            return View(pollAnswer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            PollViewModel pollViewModel = new PollViewModel();
            pollViewModel.Poll = _pollLogic.Get(id);
            pollViewModel.PollAnswers = _pollAnswerLogic.GetGeneric(x => x.PollId == id);

            if (pollViewModel.Poll == null)
            {
                return NotFound();
            }

            return View(pollViewModel);
        }

        // POST: /Admin/PageManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var poll = _pollLogic.Get(id);

            var pollAnswers = _pollAnswerLogic.GetGeneric(x => x.PollId == id);

            foreach (var pollAnswer in pollAnswers)
			{
                var pollVotes = _pollVoteLogic.GetGeneric(x => x.PollAnswerId == pollAnswer.Id);

                foreach (var pollVote in pollVotes)
				{
                    _pollVoteLogic.Delete(pollVote);
				}

                _pollAnswerLogic.Delete(pollAnswer);
            }

            _pollLogic.Delete(poll);

            return RedirectToAction("Index");
        }
    }
}