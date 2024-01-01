﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digiozPortal.BLL.Interfaces;
using digiozPortal.BO;
using digiozPortal.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace digiozPortal.Web.Controllers
{
    public class MailingListController : BaseController
    {
        private readonly ILogger<MailingListController> _logger;
        private readonly ILogic<MailingList> _mailingListLogic;
        private readonly ILogic<MailingListCampaign> _mailingListCampaignsLogic;
        private readonly ILogic<MailingListSubscriber> _mailingListSubscriberLogic;
        private readonly ILogic<MailingListSubscriberRelation> _mailingListSubscriberRelationLogic;

        public MailingListController(
            ILogger<MailingListController> logger,
            ILogic<MailingList> mailingListLogic,
            ILogic<MailingListCampaign> mailingListCampaignsLogic,
            ILogic<MailingListSubscriber> mailingListSubscriberLogic,
            ILogic<MailingListSubscriberRelation> mailingListSubscriberRelationLogic
        ) {
            _logger = logger;
            _mailingListLogic = mailingListLogic;
            _mailingListCampaignsLogic = mailingListCampaignsLogic;
            _mailingListSubscriberLogic = mailingListSubscriberLogic;
            _mailingListSubscriberRelationLogic = mailingListSubscriberRelationLogic;
        }

        // GET: MailingList
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult EmailDisplay(Guid id)
        {
            var model = _mailingListCampaignsLogic.Get(id); 

            if (model == null)
            {
                model = new MailingListCampaign {
                    Subject = string.Empty,
                    Banner = string.Empty,
                    Body = string.Empty
                };
            }

            // Update Count
            if (model.Body != null)
            {
                model.VisitorCount += 1;
                _mailingListCampaignsLogic.Edit(model);
            }

            var address = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            ViewBag.EmailDisplayUrl = address + "/MailingList/EmailDisplay/" + model.Id; 
            ViewBag.UnsubscribeUrl = address + "/MailingList/Unsubscribe";
            var bannerUrl = string.Empty;

            if (!string.IsNullOrEmpty(model.Banner)) {
                bannerUrl = address + "/Content/Emails/uploads/Full/" + model.Banner;
            }

            ViewBag.BannerUrl = bannerUrl;

            return View(model);
        }

        public ActionResult Unsubscribe()
        {
            var subscriptionVM = new UnsubscribeViewModel {
                Unsubscribe = true
            };

            return View(subscriptionVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unsubscribe([Bind("Email","Unsubscribe")]UnsubscribeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // find subscription first if any
                var subscribers = _mailingListSubscriberLogic.GetAll().Where(x => x.Email == model.Email);

                if (subscribers != null && model.Unsubscribe == true)
                {
                    foreach (var subscriber in subscribers) {
                        // remove all subscriptions 
                        var subscriberRelations = _mailingListSubscriberRelationLogic.GetAll().Where(x => x.MailingListSubscriberId == subscriber.Id).ToList();

                        foreach (var relation in subscriberRelations) {
                            _mailingListSubscriberRelationLogic.Delete(relation);
                        }

                        _mailingListSubscriberLogic.Delete(subscriber);
                    }

                }
            }

            return RedirectToAction("UnsubscribeConfirmaiton");
        }

        public ActionResult UnsubscribeConfirmaiton()
        {
            return View();
        }
    }
}