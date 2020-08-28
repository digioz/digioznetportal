using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using digiozPortal.BO;
using digiozPortal.BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using digiozPortal.BLL.Interfaces;

namespace digiozPortal.Web.Controllers
{
    public class PageController : BaseController
    {
        private readonly ILogic<Page> _pageLogic;

        public PageController(
            ILogic<Page> pageLogic
        ) {
            _pageLogic = pageLogic;
        }

        //
        // GET: /Page/
        public ActionResult Index(int id)
        {
            var loPage = _pageLogic.GetAll().SingleOrDefault(x => x.Id == id);
            return View(loPage);
        }

        public ActionResult ByID(int? id)
        {
            var loPage = _pageLogic.GetAll().SingleOrDefault(x => x.Id == id);
            return View(loPage);
        }

        public ActionResult ByName(string name)
        {
            var loPage = new Page();

            try
            {
                loPage = _pageLogic.GetAll().SingleOrDefault(x => x.URL == name);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

            if (loPage == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(loPage);
        }
	}
}