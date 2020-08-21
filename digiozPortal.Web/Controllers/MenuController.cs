using System.Collections.Generic;
using digiozPortal.BO;
using digiozPortal.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using digiozPortal.Utilities;
using digiozPortal.Web.Models.ViewModels;
using digiozPortal.Web.Helpers;
using digiozPortal.BLL.Interfaces;

namespace digiozPortal.Web.Controllers
{
    public class MenuController : BaseController
    {
        private readonly ILogic<Menu> _menuLogic;
        private readonly ILogic<Plugin> _pluginLogic;
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<Poll> _pollLogic;
        private readonly ILogic<VisitorSession> _visitorSessionLogic;
        private readonly ILogic<Module> _moduleLogic;
        private readonly ILogic<SlideShow> _slideShowLogic;
        private readonly ILogic<CommentConfig> _commentConfigLogic;
        private readonly ILogic<Picture> _pictureLogic;
        private readonly ILogic<ProductCategory> _productCategoryLogic;
        private readonly ILogic<Rss> _rssLogic;

        public MenuController(
            ILogic<Menu> menuLogic,
            ILogic<Plugin> pluginLogic,
            IConfigLogic configLogic,
            ILogic<Poll> pollLogic,
            ILogic<VisitorSession> visitorSessionLogic,
            ILogic<Module> moduleLogic,
            ILogic<SlideShow> slideShowLogic,
            ILogic<CommentConfig> commentConfigLogic,
            ILogic<Picture> pictureLogic,
            ILogic<ProductCategory> productCategoryLogic,
            ILogic<Rss> rssLogic
        ) 
        {
            _menuLogic = menuLogic;
            _pluginLogic = pluginLogic;
            _configLogic = configLogic;
            _pollLogic = pollLogic;
            _visitorSessionLogic = visitorSessionLogic;
            _moduleLogic = moduleLogic;
            _slideShowLogic = slideShowLogic;
            _commentConfigLogic = commentConfigLogic;
            _pictureLogic = pictureLogic;
            _productCategoryLogic = productCategoryLogic;
            _rssLogic = rssLogic;
        }

        //
        // GET: /Menu/
        public ActionResult Index()
        {
            return View();
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public ActionResult TopMenu()
        {
            var topMenus = _menuLogic.GetAll().Where(x => x.Location == "TopMenu" && x.Visible == true).OrderBy(x => x.SortOrder).ToList();

            return PartialView("TopMenu", topMenus);
        }

        public ActionResult UserMenu() 
        {
            return PartialView("UserMenu");
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public ActionResult PluginMenu()
        {
            var plugins = _pluginLogic.GetAll().Where(x => x.IsEnabled == true);

            return PartialView("PluginMenu", plugins);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult LeftMenu()
        {
            //var logic = new MenuLogic();
            var leftMenus = _menuLogic.GetAll().Where(x => x.Location == "LeftMenu" && x.Visible == true).OrderBy(x => x.SortOrder).ToList();

            return PartialView("LeftMenu", leftMenus);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult StoreMenu()
        {
            var plugins = _pluginLogic.GetAll().Where(x => x.Name == "Store" && x.IsEnabled == true).ToList();
            ViewBag.ShowStore = false;

            if (plugins.Count > 0)
            {
                ViewBag.ShowStore = true;
            }

            var productCategories = _productCategoryLogic.GetAll(); 

            return PartialView("StoreMenu", productCategories);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult PollMenu()
        {
            var polls = _pollLogic.GetAll().Where(x => x.Featured == true);
            polls = polls.OrderByDescending(x => x.DateCreated).ToList();
            var poll = polls.Take(1).SingleOrDefault();

            Response.ContentType = "text/html";

            return PartialView("PollMenu", poll);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult TwitterMenu()
        {
            var plugins = _pluginLogic.GetAll().Where(x => x.IsEnabled == true && x.Name == "Twitter");

            if (plugins.Any())
            {
                //Check config table for twitter handle
                var configs = _configLogic.GetAll().Where(x => x.ConfigKey == "TwitterHandle");
                var twitterHandleConfig = configs.Take(1).SingleOrDefault();
                configs = _configLogic.GetAll().Where(x => x.ConfigKey == "TwitterWidgetID");

                if (twitterHandleConfig != null)
                {
                    var twitterFeed = new Twitter(twitterHandleConfig.ConfigValue, true);
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
            var configWhoIsOnline = _pluginLogic.GetAll().SingleOrDefault(x => x.Name == "WhoIsOnline");

            var latestVisitors = _visitorSessionLogic.GetAll().Where(x => x.DateModified >= DateTime.Now.AddMinutes(-10)).ToList();
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
            var modules = _moduleLogic.GetAll().Where(x => x.Location == zoneType).ToList();
            ViewBag.SelectedZone = zoneType;

            return PartialView("ZoneMenu", modules);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public ActionResult SlideShow()
        {
            var slides = new List<SlideShow>();
            var plugins = _pluginLogic.GetAll().SingleOrDefault(x => x.IsEnabled == true &&  x.Name == "SlideShow");

            if (plugins != null)
            {
                slides = _slideShowLogic.GetAll();
            }

            return PartialView("SlideShow", slides);
        }

        public ActionResult CommentsMenu(string referenceType, string referenceId)
        {
            var commentVM = new CommentsMenuViewModel {
                ReferenceId = Convert.ToInt32(referenceId),
                ReferenceType = referenceType,
                Count = 0,
                Likes = 0,
                CommentsEnabled = false
            };

            var plugins = _pluginLogic.GetAll().SingleOrDefault(x => x.IsEnabled == true && x.Name == "Comments");
            var configs = _configLogic.GetAll().Where(x => x.ConfigKey == "EnableCommentsOnAllPages");
            var enableCommentsOnAllPages = configs.Take(1).SingleOrDefault();

            if (enableCommentsOnAllPages != null && enableCommentsOnAllPages.ConfigValue == "true")
            {
                commentVM.CommentsEnabled = true;
            }
            else
            {
                var commentConfigs = _commentConfigLogic.GetAll().Where(x => x.ReferenceId == referenceId && x.ReferenceType == referenceType).ToList();

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
            var plugins = _pluginLogic.GetAll().Where(x => x.IsEnabled == true && x.Name == "RSSFeed");
            var feedContent = new List<RSSViewModel>();

            var rssList = _rssLogic.GetAll();

            if (plugins.Any())
            {
                feedContent = Utility.GetRSSFeeds(rssList);
            }

            return PartialView("RSSFeed", feedContent);
        }

        public ActionResult LatestPictures()
        {
            var plugins = _pluginLogic.GetAll().Where(x => x.IsEnabled == true && x.Name == "LatestPictures");
            var latestPictures = new List<Picture>();
            ViewBag.ShowLatestPictures = false;

            if (plugins.Any())
            {
                latestPictures = _pictureLogic.GetAll().Take(9).ToList();
                ViewBag.ShowLatestPictures = true;
            }

            return PartialView("LatestPictures", latestPictures);
        }
    }
}