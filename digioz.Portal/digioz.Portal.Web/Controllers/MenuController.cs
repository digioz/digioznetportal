using System.Collections.Generic;
using digioz.Portal.Bo;
using digioz.Portal.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Models.ViewModels;
using digioz.Portal.Web.Helpers;
using digioz.Portal.Bll.Interfaces;
using System.Threading.Tasks;
using digioz.Portal.Bll;

namespace digioz.Portal.Web.Controllers
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
        private readonly ILogic<Video> _videoLogic;
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
            ILogic<Video> videoLogic,
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
            _videoLogic = videoLogic;
            _productCategoryLogic = productCategoryLogic;
            _rssLogic = rssLogic;
        }

        //
        // GET: /Menu/
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public async Task<IActionResult> TopMenu()
        {
            var topMenus = _menuLogic.GetGeneric(x => x.Location == "TopMenu" && x.Visible == true).OrderBy(x => x.SortOrder).ToList();

            return PartialView("TopMenu", topMenus);
        }

        public async Task<IActionResult> UserMenu() 
        { 
            return PartialView("UserMenu");
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public async Task<IActionResult> PluginMenu()
        {
            var plugins = _pluginLogic.GetGeneric(x => x.IsEnabled == true);

            return PartialView("PluginMenu", plugins);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public async Task<IActionResult> LeftMenu()
        {
            //var logic = new MenuLogic();
            var leftMenus = _menuLogic.GetGeneric(x => x.Location == "LeftMenu" && x.Visible == true).OrderBy(x => x.SortOrder).ToList();

            return PartialView("LeftMenu", leftMenus);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public async Task<IActionResult> StoreMenu()
        {
            var plugins = _pluginLogic.GetGeneric(x => x.Name == "Store" && x.IsEnabled == true).ToList();
            ViewBag.ShowStore = false;

            if (plugins.Count > 0)
            {
                ViewBag.ShowStore = true;
            }

            var productCategories = _productCategoryLogic.GetAll(); 

            return PartialView("StoreMenu", productCategories);
        }

		[ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
		public async Task<IActionResult> PollMenu()
        {
            var polls = _pollLogic.GetGeneric(x => x.Featured == true);
            polls = polls.OrderByDescending(x => x.DateCreated).ToList();
            var poll = polls.Take(1).SingleOrDefault();

            var chartLogic = new ChartLogic();
            var pollResults = chartLogic.GetPollResults(poll.Id);
            ViewBag.PollResults = pollResults;

            return PartialView("PollMenu", poll);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public async Task<IActionResult> TwitterMenu()
        {
            var plugins = _pluginLogic.GetGeneric(x => x.IsEnabled == true && x.Name == "Twitter");

            if (plugins.Any())
            {
                //Check config table for twitter handle
                var configs = _configLogic.GetConfig();
                var twitterHandle = configs["TwitterHandle"];

                if (twitterHandle != null)
                {
                    var twitterFeed = new Twitter(twitterHandle, true);
                    ViewBag.TwitterHandle = twitterFeed.TwitterHandle;
                    ViewBag.TwitterUser = twitterFeed.TwitterUser;

                    return PartialView("TwitterMenu", twitterFeed);
                }
            }

            return PartialView("TwitterMenu", null);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public async Task<IActionResult> WhoIsOnlineMenu()
        {
            var configWhoIsOnline = _pluginLogic.GetGeneric(x => x.Name == "WhoIsOnline").SingleOrDefault();

            var latestVisitors = _visitorSessionLogic.GetGeneric(x => x.DateModified >= DateTime.Now.AddMinutes(-10)).ToList();
            var visitorRegistered = latestVisitors.Where(x => x.Username != null).DistinctBy(x => x.Username).ToList();

            ViewBag.VisitorCount = latestVisitors.Count;
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
        public async Task<IActionResult> ZoneMenu(string zoneType)
        {
            var modules = _moduleLogic.GetGeneric(x => x.Location == zoneType && x.Visible == true).ToList();
            ViewBag.SelectedZone = zoneType;

            return PartialView("ZoneMenu", modules);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public async Task<IActionResult> SlideShow()
        {
            var slides = new List<SlideShow>();
            var plugins = _pluginLogic.GetGeneric(x => x.IsEnabled == true &&  x.Name == "SlideShow").SingleOrDefault();

            if (plugins != null)
            {
                slides = _slideShowLogic.GetAll();
            }

            return PartialView("SlideShow", slides);
        }

        public async Task<IActionResult> CommentsMenu(string referenceType, string referenceId)
        {
            var commentVM = new CommentsMenuViewModel {
                ReferenceId = Convert.ToInt32(referenceId),
                ReferenceType = referenceType,
                Count = 0,
                Likes = 0,
                CommentsEnabled = false
            };

            var plugins = _pluginLogic.GetGeneric(x => x.IsEnabled == true && x.Name == "Comments").SingleOrDefault();
            var configs = _configLogic.GetGeneric(x => x.ConfigKey == "EnableCommentsOnAllPages");
            var enableCommentsOnAllPages = configs.Take(1).SingleOrDefault();

            if (enableCommentsOnAllPages != null && enableCommentsOnAllPages.ConfigValue == "true")
            {
                commentVM.CommentsEnabled = true;
            }
            else
            {
                var commentConfigs = _commentConfigLogic.GetGeneric(x => x.ReferenceId == referenceId && x.ReferenceType == referenceType).ToList();

                if (commentConfigs.Count > 0)
                {
                    commentVM.CommentsEnabled = true;
                }
            }

            return PartialView("CommentsMenu", commentVM);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public async Task<IActionResult> RSSFeed()
        {
            var plugins = _pluginLogic.GetGeneric(x => x.IsEnabled == true && x.Name == "RSSFeed");
            var feedContent = new List<RSSViewModel>();

            var rssList = _rssLogic.GetAll();

            if (plugins.Any())
            {
                feedContent = Utility.GetRSSFeeds(rssList);
            }

            return PartialView("RSSFeed", feedContent);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 20)]
        public async Task<IActionResult> LatestPictures()
        {
            var plugins = _pluginLogic.GetGeneric(x => x.IsEnabled == true && x.Name == "LatestPictures");
            var latestPictures = new List<Picture>();
            ViewBag.ShowLatestPictures = false;

            if (plugins.Any())
            {
                latestPictures = _pictureLogic.GetAll().Take(9).ToList(); // To Do - Make it more performant
                ViewBag.ShowLatestPictures = true;
            }

            return PartialView("LatestPictures", latestPictures);
        }

        [ResponseCache(VaryByHeader = "User-Agent", Duration = 20)]
        public async Task<IActionResult> LatestVideos()
        {
            var plugins = _pluginLogic.GetAll().Where(x => x.IsEnabled == true && x.Name == "LatestVideos");
            var latestVideos = new List<Video>();
            ViewBag.ShowLatestVideos = false;

            if (plugins.Any())
            {
                latestVideos = _videoLogic.GetAll().Take(9).ToList(); // ToDo - Make it more performant
                ViewBag.ShowLatestVideos = true;
            }

            return PartialView("LatestVideos", latestVideos);
        }
    }
}