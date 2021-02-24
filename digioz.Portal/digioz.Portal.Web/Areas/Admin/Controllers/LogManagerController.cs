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
	public class LogManagerController : Controller
	{
        private readonly ILogic<Log> _logLogic;

        public LogManagerController( 
            IConfigLogic configLogic,
            ILogic<Log> logLogic
        )
        {
            _logLogic = logLogic;
        }

        public async Task<IActionResult> Index()
		{
            var query = "SELECT TOP (1000) [Id],LEFT([Message],150) AS [Message], [Level], [Timestamp], '' AS [Exception], '' AS [LogEvent] FROM Log ORDER BY Id DESC;";
            var logs = (List<Log>)_logLogic.GetQueryString(query);

			return View(logs);
		}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = _logLogic.Get(id.GetValueOrDefault());

            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }
    }
}
