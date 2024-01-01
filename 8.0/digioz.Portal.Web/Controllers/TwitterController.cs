using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digioz.Portal.Bo;
using digioz.Portal.Bll;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Bll.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using digioz.Portal.Web.Models;

namespace digioz.Portal.Web.Controllers
{
    public class TwitterController : Controller
    {
        private readonly IConfigLogic _configLogic;
        private readonly ILogic<Plugin> _pluginLogic;

        public TwitterController(
            IConfigLogic configLogic,
            ILogic<Plugin> pluginLogic
        )
        {
            _configLogic = configLogic;
            _pluginLogic = pluginLogic;
        }

        // GET: Twitter
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}