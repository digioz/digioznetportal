using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class HomeController : Controller
    {
        private readonly ILogic<VisitorInfo> _visitorInfoLogic;

        public HomeController(
            ILogic<VisitorInfo> visitorInfoLogic
        )
        {
            _visitorInfoLogic = visitorInfoLogic;
        }

        public async Task<IActionResult> Index() {
            var model = new HomeViewModel();
            var query = @"SELECT TOP (1000) [Id], [IpAddress], [OperatingSystem],[Browser],[BrowserVersion], [Href], [Timestamp],
						[JavaEnabled], [ScreenHeight], [ScreenWidth], '' AS [BrowserEngineName], '' AS [Host], 
						'' AS [HostName], '' AS [Platform], '' AS [Referrer], '' AS [UserAgent], '' AS [UserLanguage], '' AS [SessionId]
						FROM [VisitorInfo] ORDER BY Id DESC;";
            model.Visitors = (List<VisitorInfo>)_visitorInfoLogic.GetQueryString(query);

            return View(model);
        }
    }
}
