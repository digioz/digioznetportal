using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MailingListSubscriberManagerController : Controller
    {
        private readonly ILogger<MailingListSubscriberManagerController> _logger;
        private readonly ILogic<Log> _logLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<MailingList> _mailingListLogic;
        private readonly ILogic<MailingListCampaign> _mailingListCampaignLogic;
        private readonly ILogic<MailingListSubscriber> _mailingListSubscriberLogic;
        private readonly ILogic<MailingListSubscriberRelation> _mailingListSubscriberRelationLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailingListSubscriberManagerController(
            ILogger<MailingListSubscriberManagerController> logger,
            ILogic<Log> logLogic,
            IConfigLogic configLogic,
            ILogic<MailingList> mailingListLogic,
            ILogic<MailingListCampaign> mailingListCampaignLogic,
            ILogic<MailingListSubscriber> mailingListSubscriberLogic,
            ILogic<MailingListSubscriberRelation> mailingListSubscriberRelationLogic,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _logger = logger;
            _logLogic = logLogic;
            _configLogic = configLogic;
            _mailingListLogic = mailingListLogic;
            _mailingListCampaignLogic = mailingListCampaignLogic;
            _mailingListSubscriberLogic = mailingListSubscriberLogic;
            _mailingListSubscriberRelationLogic = mailingListSubscriberRelationLogic;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/MailingListSubscriberManager
        public async Task<IActionResult> Index()
        {
            return View(_mailingListSubscriberLogic.GetAll());
        }

        // GET: Admin/MailingListSubscriberManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingListSubscriber mailingListSubscriber = _mailingListSubscriberLogic.Get(id);

            // Get list of Mailing Lists Subscriber belongs to
            var mailingListSubscriberRelations = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListSubscriberId == mailingListSubscriber.Id).ToList();
            var mailingListSubscribedTo = new List<MailingList>();

            foreach (var item in mailingListSubscriberRelations)
            {
                mailingListSubscribedTo.Add(_mailingListLogic.GetGeneric(x => x.Id == item.MailingListId).SingleOrDefault());
            }

            ViewBag.MailingListSubscribedTo = mailingListSubscribedTo.OrderBy(x => x.Name);

            if (mailingListSubscriber == null)
            {
                return NotFound();
            }

            return View(mailingListSubscriber);
        }

        // GET: Admin/MailingListSubscriberManager/Create
        public async Task<IActionResult> Create()
        {
            var mailingList = _mailingListLogic.GetAll();
            ViewBag.MailingLists = mailingList.OrderBy(x => x.Name);

            return View();
        }

        // POST: Admin/MailingListSubscriberManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,FirstName,LastName,Status,DateCreated,DateModified")] MailingListSubscriber mailingListSubscriber, IFormCollection form)
        {
            // set dates for the record
            mailingListSubscriber.DateCreated = DateTime.Now;
            mailingListSubscriber.DateModified = DateTime.Now;

            if (ModelState.IsValid)
            {
                mailingListSubscriber.Id = Guid.NewGuid().ToString();
                _mailingListSubscriberLogic.Add(mailingListSubscriber);

                // Now save mailing list record(s)
                var mailingLists = form["mailinglist"].ToArray();
                List<MailingListSubscriberRelation> mailingSubscriberRelations = new List<MailingListSubscriberRelation>();

                if (mailingLists != null)
                {
                    foreach (var item in mailingLists)
                    {
                        MailingListSubscriberRelation mailingListSubscriberRelation = new MailingListSubscriberRelation();
                        mailingListSubscriberRelation.Id = Guid.NewGuid().ToString();
                        mailingListSubscriberRelation.MailingListId = new Guid(item).ToString();
                        mailingListSubscriberRelation.MailingListSubscriberId = mailingListSubscriber.Id;

                        mailingSubscriberRelations.Add(mailingListSubscriberRelation);
                    }

                    foreach(var mailingSubscriberRelation in mailingSubscriberRelations)
                    {
                        _mailingListSubscriberRelationLogic.Add(mailingSubscriberRelation);
                    }
                }

                return RedirectToAction("Index");
            }

            var mailingList = _mailingListLogic.GetAll().OrderBy(x => x.Name).ToList();
            ViewBag.MailingLists = mailingList;

            return View(mailingListSubscriber);
        }

        // GET: Admin/MailingListSubscriberManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            // Get total list of mailing lists
            var mailingLists = _mailingListLogic.GetAll().OrderBy(x => x.Name).ToList();
            ViewBag.MailingLists = mailingLists;

            MailingListSubscriber mailingListSubscriber = _mailingListSubscriberLogic.Get(id);

            // Get a list of mailing lists user is subscribed to
            var mailingListsSubscribed = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListSubscriberId == mailingListSubscriber.Id).ToList();
            List<string> subscribed = new List<string>();

            foreach (var item in mailingListsSubscribed)
            {
                subscribed.Add(item.MailingListId.ToString());
            }

            ViewBag.MailingListsSubscribed = subscribed;

            if (mailingListSubscriber == null)
            {
                return NotFound();
            }

            return View(mailingListSubscriber);
        }

        // POST: Admin/MailingListSubscriberManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Email,FirstName,LastName,Status,DateCreated,DateModified")] MailingListSubscriber mailingListSubscriber, FormCollection form)
        {
            // Get total list of mailing lists
            var mailingLists = _mailingListLogic.GetAll().OrderBy(x => x.Name).ToList();
            ViewBag.MailingLists = mailingLists;

            mailingListSubscriber.DateModified = DateTime.Now;

            if (ModelState.IsValid)
            {
                _mailingListSubscriberLogic.Edit(mailingListSubscriber);

                // Wipe out previous mailing lists subscribed to first
                var subscriberRelations = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListSubscriberId == mailingListSubscriber.Id).ToList();

                if (subscriberRelations.Count > 0)
                {
                    foreach (var subscriberRelation in subscriberRelations)
                    {
                        _mailingListSubscriberRelationLogic.Delete(subscriberRelation);
                    }
                }

                // Now save the new mailing list record(s)
                var mailingListSelected = form["mailinglist"].ToArray();
                List<MailingListSubscriberRelation> mailingSubscriberRelations = new List<MailingListSubscriberRelation>();

                if (mailingListSelected != null)
                {
                    foreach (var item in mailingListSelected)
                    {
                        MailingListSubscriberRelation mailingListSubscriberRelation = new MailingListSubscriberRelation();
                        mailingListSubscriberRelation.Id = Guid.NewGuid().ToString();
                        mailingListSubscriberRelation.MailingListId = new Guid(item).ToString();
                        mailingListSubscriberRelation.MailingListSubscriberId = mailingListSubscriber.Id;

                        mailingSubscriberRelations.Add(mailingListSubscriberRelation);
                    }

                    foreach(var mailingSubscriberRelation in mailingSubscriberRelations)
                    {
                        _mailingListSubscriberRelationLogic.Add(mailingSubscriberRelation);
                    }
                }

                return RedirectToAction("Index");
            }

            // ----------------- If there is a validation error - Start ----------------
            
            // Get a list of mailing lists user is subscribed to
            var mailingListsSubscribed = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListSubscriberId == mailingListSubscriber.Id).ToList();
            List<string> subscribed = new List<string>();

            foreach (var item in mailingListsSubscribed)
            {
                subscribed.Add(item.MailingListId.ToString());
            }

            ViewBag.MailingListsSubscribed = subscribed;

            // ----------------- If there is a validation error - End ----------------

            return View(mailingListSubscriber);
        }

        // GET: Admin/MailingListSubscriberManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingListSubscriber mailingListSubscriber = _mailingListSubscriberLogic.Get(id);

            // Get list of Mailing Lists Subscriber belongs to
            var mailingListSubscriberRelations = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListSubscriberId == mailingListSubscriber.Id).ToList();
            var mailingListSubscribedTo = new List<MailingList>();

            foreach (var item in mailingListSubscriberRelations)
            {
                mailingListSubscribedTo.Add(_mailingListLogic.GetGeneric(x => x.Id == item.MailingListId).SingleOrDefault());
            }

            ViewBag.MailingListSubscribedTo = mailingListSubscribedTo.OrderBy(x => x.Name);


            if (mailingListSubscriber == null)
            {
                return NotFound();
            }

            return View(mailingListSubscriber);
        }

        // POST: Admin/MailingListSubscriberManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            MailingListSubscriber mailingListSubscriber = _mailingListSubscriberLogic.Get(id);

            // Remove Mailing List Records first
            var subscriberRelations = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListSubscriberId == mailingListSubscriber.Id).ToList();

            if (subscriberRelations.Count > 0)
            {
                foreach (var subscriberRelation in subscriberRelations)
                {
                    _mailingListSubscriberRelationLogic.Delete(subscriberRelation);
                }
            }

            _mailingListSubscriberLogic.Delete(mailingListSubscriber);

            return RedirectToAction("Index");
        }
    }
}
