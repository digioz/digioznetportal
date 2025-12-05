using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.SlideShow
{
    public class IndexModel : PageModel
    {
        private readonly ISlideShowService _service;
        public IndexModel(ISlideShowService service) { _service = service; }

        public IReadOnlyList<digioz.Portal.Bo.SlideShow> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.SlideShow>();
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, pageSize));

        public void OnGet()
        {
            var all = _service.GetAll().OrderByDescending(s => s.DateModified).ThenByDescending(s => s.DateCreated).ToList();
            TotalCount = all.Count;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;
            Items = all.Skip(skip).Take(pageSize).ToList();
        }
    }
}
