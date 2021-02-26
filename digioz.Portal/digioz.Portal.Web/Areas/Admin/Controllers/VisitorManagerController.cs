using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Administrator")]
	public class VisitorManagerController : Controller
	{
		private readonly ILogic<VisitorInfo> _visitorInfoLogic;

		public VisitorManagerController(
			ILogic<VisitorInfo> visitorInfoLogic
		)
		{
			_visitorInfoLogic = visitorInfoLogic;
		}

		public async Task<IActionResult> Index()
		{
			var query = @"SELECT TOP (1000) [Id], [IpAddress], [OperatingSystem],[Browser],[BrowserVersion], [Href], [Timestamp],
						[JavaEnabled], [ScreenHeight], [ScreenWidth], '' AS [BrowserEngineName], '' AS [Host], 
						'' AS [HostName], '' AS [Platform], '' AS [Referrer], '' AS [UserAgent], '' AS [UserLanguage], '' AS [SessionId]
						FROM [VisitorInfo] ORDER BY Id DESC;";
			var logs = (List<VisitorInfo>)_visitorInfoLogic.GetQueryString(query);

			return View(logs);
		}

		public async Task<IActionResult> Details(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var log = _visitorInfoLogic.Get(id.GetValueOrDefault());

			if (log == null)
			{
				return NotFound();
			}

			return View(log);
		}
	}
}
