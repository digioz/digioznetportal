using System.Collections.Generic;
using System.Diagnostics;
using digiozPortal.BO;
using digiozPortal.BLL;
using digiozPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using digiozPortal.Utilities;
using digiozPortal.Web.Models.ViewModels;
using digiozPortal.Web.Helpers;

namespace digiozPortal.Web.Controllers
{
    public class MenuController : BaseController
    {
        //
        // GET: /Menu/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TopMenu()
        {
            var logic = new MenuLogic();
            var topMenus = logic.GetAll();; //.Where(x => x.Location == "TopMenu" && x.Visible == true).OrderBy(x => x.SortOrder).ToList();

            return PartialView("TopMenu", topMenus);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public ActionResult PluginMenu()
        {
            var logic = new PluginLogic();
            var plugins = logic.GetAll().Where(x => x.IsEnabled == true);

            return PartialView("PluginMenu", plugins);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult LeftMenu()
        {
            var logic = new MenuLogic();
            var leftMenus = logic.GetAll().Where(x => x.Location == "LeftMenu" && x.Visible == true).OrderBy(x => x.SortOrder).ToList();

            return PartialView("LeftMenu", leftMenus);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult StoreMenu()
        {
            var logic = new PluginLogic();
            var plugins = logic.GetAll().Where(x => x.Name == "Store" && x.IsEnabled == true).ToList();
            ViewBag.ShowStore = false;

            if (plugins.Count > 0)
            {
                ViewBag.ShowStore = true;
            }

            var logicProductCategory = new ProductCategoryLogic(); // ProductCategoryLogic();
            var productCategories = logicProductCategory.GetAll(); 

            return PartialView("StoreMenu", productCategories);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult PollMenu()
        {
            var logic = new PollLogic();
            var polls = logic.GetAll().Where(x => x.Featured == true);
            polls = polls.OrderByDescending(x => x.DateCreated).ToList();
            var poll = polls.Take(1).SingleOrDefault();

            Response.ContentType = "text/html";

            return PartialView("PollMenu", poll);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult TwitterMenu()
        {
            var logic = new PluginLogic();
            var plugins = logic.GetAll().Where(x => x.IsEnabled == true && x.Name == "Twitter");

            if (plugins.Any())
            {
                //Check config table for twitter handle
                var logicConfig = new ConfigLogic();
                var configs = logicConfig.GetAll().Where(x => x.ConfigKey == "TwitterHandle");
                var twitterHandleConfig = configs.Take(1).SingleOrDefault();
                configs = logicConfig.GetAll().Where(x => x.ConfigKey == "TwitterWidgetID");

                if (twitterHandleConfig != null)
                {
                    Twitter twitterFeed = new Twitter(twitterHandleConfig.ConfigValue, true);
                    ViewBag.TwitterHandle = twitterFeed.TwitterHandle;
                    ViewBag.TwitterUser = twitterFeed.TwitterUser;
                    return PartialView("TwitterMenu", twitterFeed);
                }
            }
            return null;
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult WhoIsOnlineMenu()
        {
            var logic = new PluginLogic();
            var configWhoIsOnline = logic.GetAll().SingleOrDefault(x => x.Name == "WhoIsOnline");

            var logicVisitors = new VisitorSessionLogic();
            var latestVisitors = logicVisitors.GetAll().Where(x => x.DateModified >= DateTime.Now.AddMinutes(-10)).ToList();
            var visitorRegistered = latestVisitors.Where(x => x.UserName != null).DistinctBy(x => x.UserName).ToList();

            ViewBag.VisitorCount = latestVisitors.Count();
            ViewBag.RegisteredVisitors = visitorRegistered;

            if (configWhoIsOnline == null || configWhoIsOnline.IsEnabled == false)
            {
                ViewBag.WhoIsOnlineEnabled = false;
            }
            else
            {
                ViewBag.WhoIsOnlineEnabled = true;
            }
            
            return PartialView("WhoIsOnlineMenu");
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult ZoneMenu(string zoneType)
        {
            var logic = new ModuleLogic();
            var modules = logic.GetAll().Where(x => x.Location == zoneType).ToList();
            ViewBag.SelectedZone = zoneType;

            return PartialView("ZoneMenu", modules);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult SlideShow()
        {
            var slides = new List<SlideShow>();
            var logic = new PluginLogic();
            var plugins = logic.GetAll().SingleOrDefault(x => x.IsEnabled == true &&  x.Name == "SlideShow");

            if (plugins != null)
            {
                var logicSlideShow = new SlideShowLogic();
                slides = logicSlideShow.GetAll();
            }

            return PartialView("SlideShow", slides);
        }

        public ActionResult CommentsMenu(string referenceType, string referenceId)
        {
            CommentsMenuViewModel commentVM = new CommentsMenuViewModel();
            commentVM.ReferenceId = referenceId;
            commentVM.ReferenceType = referenceType;
            commentVM.Count = 0;
            commentVM.Likes = 0;
            commentVM.CommentsEnabled = false;

            var logic = new PluginLogic();
            var plugins = logic.GetAll().SingleOrDefault(x => x.IsEnabled == true && x.Name == "Comments");
            var logicConfig = new ConfigLogic();
            var configs = logicConfig.GetAll().Where(x => x.ConfigKey == "EnableCommentsOnAllPages");
            var enableCommentsOnAllPages = configs.Take(1).SingleOrDefault();

            if (enableCommentsOnAllPages != null && enableCommentsOnAllPages.ConfigValue == "true")
            {
                commentVM.CommentsEnabled = true;
            }
            else
            {
                var logicCommentConfig = new CommentConfigLogic();
                var commentConfigs = logicCommentConfig.GetAll().Where(x => x.ReferenceId == referenceId && x.ReferenceType == referenceType).ToList();

                if (commentConfigs.Count > 0)
                {
                    commentVM.CommentsEnabled = true;
                }
            }

            return PartialView("CommentsMenu", commentVM);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public ActionResult RSSFeed()
        {
            var logic = new PluginLogic();
            var plugins = logic.GetAll().Where(x => x.IsEnabled == true && x.Name == "RSSFeed");
            var feedContent = new List<RSSViewModel>();

            if (plugins.Any())
            {
                feedContent = Utility.GetRSSFeeds();
            }

            return PartialView("RSSFeed", feedContent);
        }

        public ActionResult LatestPictures()
        {
            var logic = new PluginLogic();
            var plugins = logic.GetAll().Where(x => x.IsEnabled == true && x.Name == "LatestPictures");
            var latestPictures = new List<Picture>();
            ViewBag.ShowLatestPictures = false;

            if (plugins.Any())
            {
                var logicPicture = new PictureLogic();
                latestPictures = logicPicture.GetAll().Take(9).ToList();
                ViewBag.ShowLatestPictures = true;
            }

            return PartialView("LatestPictures", latestPictures);
        }
    }
}