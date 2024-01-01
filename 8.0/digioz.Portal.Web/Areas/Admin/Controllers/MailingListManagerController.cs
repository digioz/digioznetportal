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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MailingListManagerController : Controller
    {
        private readonly ILogger<MailingListManagerController> _logger;
        private readonly ILogic<Log> _logLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<MailingList> _mailingListLogic;
        private readonly ILogic<MailingListCampaign> _mailingListCampaignLogic;
        private readonly ILogic<MailingListSubscriber> _mailingListSubscriberLogic;
        private readonly ILogic<MailingListSubscriberRelation> _mailingListSubscriberRelationLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailingListManagerController(
            ILogger<MailingListManagerController> logger,
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

        // GET: Admin/MailingListManager
        public async Task<IActionResult> Index()
        {
            return View(_mailingListLogic.GetAll());
        }

        public async Task<IActionResult> List()
        {
            return View(_mailingListLogic.GetAll());
        }

        // GET: Admin/MailingListManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingList mailingList = _mailingListLogic.Get(id);

            if (mailingList == null)
            {
                return NotFound();
            }

            return View(mailingList);
        }

        // GET: Admin/MailingListManager/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Admin/MailingListManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DefaultEmailFrom,DefaultFromName,Description,Address")] MailingList mailingList)
        {
            if (ModelState.IsValid)
            {
                mailingList.Id = Guid.NewGuid().ToString();
                _mailingListLogic.Add(mailingList);

                return RedirectToAction("List");
            }

            return View(mailingList);
        }

        // GET: Admin/MailingListManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingList mailingList = _mailingListLogic.Get(id);

            if (mailingList == null)
            {
                return NotFound();
            }

            return View(mailingList);
        }

        // POST: Admin/MailingListManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,DefaultEmailFrom,DefaultFromName,Description,Address")] MailingList mailingList)
        {
            if (ModelState.IsValid)
            {
                _mailingListLogic.Edit(mailingList);

                return RedirectToAction("List");
            }

            return View(mailingList);
        }

        // GET: Admin/MailingListManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingList mailingList = _mailingListLogic.Get(id);

            if (mailingList == null)
            {
                return NotFound();
            }

            return View(mailingList);
        }

        // POST: Admin/MailingListManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            MailingList mailingList = _mailingListLogic.Get(id);
            _mailingListLogic.Delete(mailingList);

            return RedirectToAction("List");
        }
    }
}
