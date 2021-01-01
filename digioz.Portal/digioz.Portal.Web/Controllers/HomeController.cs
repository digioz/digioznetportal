using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bll;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Helpers;
using digioz.Portal.Web.Models;
using digioz.Portal.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ILogic<Page> _pageLogic;
        private readonly ILogic<Config> _configLogic;
        private readonly ILogic<Announcement> _announcementLogic;

        public HomeController(
            ILogger<HomeController> logger,
            ILogic<Page> pageLogic,
            ILogic<Config> configLogic,
            ILogic<Announcement> announcementLogic
        ) 
        {
            _logger = logger;
            _pageLogic = pageLogic;
            _configLogic = configLogic;
            _announcementLogic = announcementLogic;
        }

        public async Task<IActionResult> Index() {

            var model = new HomeIndexViewModel {
                Page = _pageLogic.GetAll().SingleOrDefault(x => x.Title == "Home" && x.Visible == true)
            };
            var numberOfAnnouncements = 1;
            var configs = _configLogic.GetAll().Where(x => x.ConfigKey == "NumberOfAnnouncements");
            if (configs != null) {
                numberOfAnnouncements = int.Parse(configs.Take(1).SingleOrDefault().ConfigValue);
            }

            model.Announcements = _announcementLogic.GetAll().OrderByDescending(x => x.Id).Where(x => x.Visible == true).Take(numberOfAnnouncements).ToList();
            return View(model);
        }

        public async Task<IActionResult> Privacy() {
            return View();
        }

        public async Task<IActionResult> About() {
            var page = _pageLogic.GetAll().SingleOrDefault(x => x.Title == "About" && x.Visible == true);
            return View(page);
        }

        public async Task<IActionResult> Contact() {
            ViewBag.ShowContactForm = false;

            var configs = _configLogic.GetAll().Where(x => x.ConfigKey == "ShowContactForm");

            if (configs.Any()) {
                var singleOrDefault = configs.Take(1).SingleOrDefault();

                if (singleOrDefault != null) {
                    var showContactForm = singleOrDefault.ConfigValue;

                    if (showContactForm == "true") {
                        ViewBag.ShowContactForm = true;
                    }
                }
            }

            var page = _pageLogic.GetAll().SingleOrDefault(x => x.Title == "Contact" && x.Visible == true);
            return View(page);
        }

        public async Task<IActionResult> Announcements() {
            var announcements = _announcementLogic.GetAll().OrderByDescending(x => x.Id).Where(x => x.Visible == true).ToList();
            return View(announcements);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error() {
            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
