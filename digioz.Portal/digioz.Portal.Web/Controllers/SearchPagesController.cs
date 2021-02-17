using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using digioz.Portal.Web.Helpers;
using digioz.Portal.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Controllers
{
	public class SearchPagesController : Controller
	{
        private readonly ILogic<Page> _pageLogic;

        public SearchPagesController(
            ILogic<Page> pageLogic
        )
        {
            _pageLogic = pageLogic;
        }

        public ActionResult Index(string searchString = "")
        {
            if (searchString == "")
            {
                RedirectToAction("Index", "Home");
            }

            List<Page> searchResults = _pageLogic.GetGeneric(x => x.Body.Contains(searchString) || x.Title.Contains(searchString) && x.Visible == true);
            List<SearchPagesViewModel> searchResultsVm = new List<SearchPagesViewModel>();

            foreach (var item in searchResults)
            {
                var searchResult = new SearchPagesViewModel
                {
                    Id = item.Id,
                    Title = item.Title
                };

                if (item.Url.Contains("/"))
                {
                    searchResult.Url = item.Url;
                }
                else
                {
                    searchResult.Url = "/page/" + item.Url;
                }

                searchResult.Body = HtmlRemoval.StripTagsRegex(item.Body).TruncateDotDotDot(200);
                searchResult.Keywords = item.Keywords;
                searchResult.Description = item.Description;

                searchResultsVm.Add(searchResult);
            }

            return View(searchResultsVm);
        }
    }
}
