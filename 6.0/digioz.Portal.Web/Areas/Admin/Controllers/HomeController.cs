using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bll;
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
        private readonly ILogic<Log> _logLogic;

        public HomeController(
            ILogic<VisitorInfo> visitorInfoLogic,
            ILogic<Log> logLogic
        )
        {
            _visitorInfoLogic = visitorInfoLogic;
            _logLogic = logLogic;
        }

        public async Task<IActionResult> Index() {
            var model = new HomeViewModel();
            var query = @"SELECT TOP (1000) [Id], [IpAddress], [OperatingSystem],[Browser],[BrowserVersion], [Href], [Timestamp],
						[JavaEnabled], [ScreenHeight], [ScreenWidth], '' AS [BrowserEngineName], '' AS [Host], 
						'' AS [HostName], '' AS [Platform], '' AS [Referrer], '' AS [UserAgent], '' AS [UserLanguage], '' AS [SessionId]
						FROM [VisitorInfo] ORDER BY Id DESC;";
            model.Visitors = (List<VisitorInfo>)_visitorInfoLogic.GetQueryString(query);

            // Get Visitor Yearly Chart
            var chartLogic = new ChartLogic();
            model.VisitorYearlyHits = chartLogic.GetVisitorYearlyHits();

            // Get Visitor Monthly Chart
            model.VisitorMonthlyHits = chartLogic.GetVisitorMonthlyHits();

            // Get Log Counts
            model.GetLogCounts = chartLogic.GetLogCounts();

            return View(model);
        }

        public async Task<IActionResult> Details(string id)
		{
            var query = "SELECT TOP (1000) [Id],LEFT([Message],150) AS [Message], [Level], [Timestamp], '' AS [Exception], '' AS [LogEvent] FROM Log ORDER BY Id DESC;";
            var logs = (List<Log>)_logLogic.GetQueryString(query);

            if (id != "All")
			{
                logs = logs.Where(x => x.Level == id).ToList();
			}

            return View(logs);
		}
    }
}
