using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using digioz.Portal.Web.Helpers;
using digioz.Portal.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MailingListCampaignManagerController : Controller
    {
        private readonly ILogger<MailingListCampaignManagerController> _logger;
        private readonly ILogic<Log> _logLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<MailingList> _mailingListLogic;
        private readonly ILogic<MailingListCampaign> _mailingListCampaignLogic;
        private readonly ILogic<MailingListSubscriber> _mailingListSubscriberLogic;
        private readonly ILogic<MailingListSubscriberRelation> _mailingListSubscriberRelationLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailingListCampaignManagerController(
            ILogger<MailingListCampaignManagerController> logger,
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

        private string GetImageFolderPath()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, "img");

            return path;
        }

        // GET: Admin/MailingListCampaignManager
        public async Task<IActionResult> Index()
        {
            return View(_mailingListCampaignLogic.GetAll());
        }

        // GET: Admin/MailingListCampaignManager/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingListCampaign mailingListCampaign = _mailingListCampaignLogic.Get(id);

            if (mailingListCampaign == null)
            {
                return NotFound();
            }

            return View(mailingListCampaign);
        }

        // GET: Admin/MailingListCampaignManager/Create
        public async Task<IActionResult> Create()
        {
            var mailingList = _mailingListLogic.GetAll();
            ViewBag.MailingLists = mailingList.OrderBy(x => x.Name);

            // Get list of banners
            List<MailingListImageViewModel> imageList = new List<MailingListImageViewModel>();

            var imgFolder = GetImageFolderPath();
            var searchFolder = Path.Combine(imgFolder, "Emails", "uploads","Full");
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = Utility.GetFilesFrom(searchFolder, filters, false);

            foreach (var item in files)
            {
                MailingListImageViewModel file = new MailingListImageViewModel();
                file.Name = item;

                imageList.Add(file);
            }

            ViewBag.Banner = imageList;

            return View();
        }

        // POST: Admin/MailingListCampaignManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Subject,FromName,FromEmail,Summary,Banner,Body,VisitorCount")] MailingListCampaign mailingListCampaign, IFormCollection form)
        {
            var config = _configLogic.GetConfig();
            var mailingLists = form["mailinglist"].ToArray();

            if (ModelState.IsValid && mailingLists != null)
            {
                // Save the record
                mailingListCampaign.Id = Guid.NewGuid().ToString();
                mailingListCampaign.DateCreated = DateTime.Now;

                _mailingListCampaignLogic.Add(mailingListCampaign);

                // Generate Email HTML based on Template
                Uri url = new Uri(Request.GetDisplayUrl()); // System.Web.HttpContext.Current.Request.Url;
                string urlLink = url.OriginalString.Replace(url.PathAndQuery, "");
                string baseUrl = String.Concat(urlLink, "/");

                string templateParsed = Helpers.Utility.GetWebContent(baseUrl + "MailingList/EmailDisplay/" + mailingListCampaign.Id);
                templateParsed = templateParsed.Trim();
                string[] stringSeparators = new string[] {"<!--"};
                var stringSeparatorsResult = templateParsed.Split(stringSeparators, StringSplitOptions.None);
                templateParsed = stringSeparatorsResult[0] + "</body></html>";
                
                // Fetch list of emails to send out
                List<string> subscriberIdListToEmail = new List<string>();

                foreach (var item in mailingLists)
                {
                    var mailingListObject = _mailingListLogic.GetGeneric(x => x.Id == item).SingleOrDefault();

                    if (mailingListObject != null)
                    {
                        var subscribers = _mailingListSubscriberRelationLogic.GetGeneric(x => x.MailingListId == mailingListObject.Id).ToList();

                        foreach (var subscriber in subscribers)
                        {
                            if (!subscriberIdListToEmail.Contains(subscriber.MailingListSubscriberId))
                            {
                               subscriberIdListToEmail.Add(subscriber.MailingListSubscriberId); 
                            }
                        }
                    }
                }

                // Send the emails out
                EmailModel email = new EmailModel();
                email.FromEmail = mailingListCampaign.FromEmail;
                email.Subject = mailingListCampaign.Subject;
                email.Message = templateParsed;
                email.SMTPServer = config["SMTPServer"];

                if (!string.IsNullOrEmpty(config["SMTPPort"]))
                {
                    email.SMTPPort = Convert.ToInt32(config["SMTPPort"]);
                }

                // Decrypt SMTP Password
                string encryptionKey = config["SiteEncryptionKey"];
                var encryptString = new EncryptString();

                email.SMTPUsername = config["SMTPUsername"];
                email.SMTPPassword = encryptString.Decrypt(encryptionKey, config["SMTPPassword"].ToString());

                foreach (var subsciber in subscriberIdListToEmail)
                {
                    var subscriberDetail = _mailingListSubscriberLogic.GetGeneric(x => x.Id == subsciber).SingleOrDefault();

                    if (subscriberDetail != null)
                    {
                        email.ToEmail = subscriberDetail.Email;

                        try
                        {
                            Helpers.Utility.SubmitMail(email, _logLogic);
                        }
                        catch (Exception ex)
                        {
                            Utility.AddLogEntry(ex.Message, _logLogic);
                        }
                    }
                }
              
                return RedirectToAction("Index");
            }

            // If it fails, populate the mailing list again
            var mailingList = _mailingListLogic.GetAll();
            ViewBag.MailingLists = mailingList.OrderBy(x => x.Name);

            return View(mailingListCampaign);
        }

        // GET: Admin/MailingListCampaignManager/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingListCampaign mailingListCampaign = _mailingListCampaignLogic.Get(id);

            if (mailingListCampaign == null)
            {
                return NotFound();
            }

            return View(mailingListCampaign);
        }

        // POST: Admin/MailingListCampaignManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Subject,FromName,FromEmail,Summary,Banner,Body,VisitorCount,DateCreated")] MailingListCampaign mailingListCampaign)
        {
            if (ModelState.IsValid)
            {
                _mailingListCampaignLogic.Edit(mailingListCampaign);

                return RedirectToAction("Index");
            }

            return View(mailingListCampaign);
        }

        // GET: Admin/MailingListCampaignManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            MailingListCampaign mailingListCampaign = _mailingListCampaignLogic.Get(id); // db.MailingListCampaigns.Find(id);

            if (mailingListCampaign == null)
            {
                return NotFound();
            }

            return View(mailingListCampaign);
        }

        // POST: Admin/MailingListCampaignManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            MailingListCampaign mailingListCampaign = _mailingListCampaignLogic.Get(id);
            _mailingListCampaignLogic.Delete(mailingListCampaign);

            return RedirectToAction("Index");
        }
    }
}
