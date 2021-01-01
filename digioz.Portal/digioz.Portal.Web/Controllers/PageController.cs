using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digioz.Portal.Bo;
using digioz.Portal.Bll;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using digioz.Portal.Bll.Interfaces;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Controllers
{
    public class PageController : BaseController
    {
        private readonly ILogic<Page> _pageLogic;

        public PageController(
            ILogic<Page> pageLogic
        ) {
            _pageLogic = pageLogic;
        }

        [Route("/page/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var loPage = new Page();

            try {
                if (Utilities.StringUtils.IsNumeric(id)) {
                    loPage = _pageLogic.GetAll().SingleOrDefault(x => x.Id == Convert.ToInt32(id));
                } else {
                    loPage = _pageLogic.GetAll().SingleOrDefault(x => x.Url.ToLower() == id.ToLower());
                }
            } catch {
                return RedirectToAction("Index", "Home");
            }

            if (loPage == null) {
                return RedirectToAction("Index", "Home");
            }

            return View(loPage);
        }
	}
}