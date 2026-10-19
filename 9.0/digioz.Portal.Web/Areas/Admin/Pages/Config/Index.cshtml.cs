using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Config
{
    public class IndexModel : PageModel
    {
        private readonly IConfigService _service;
        public IndexModel(IConfigService service) { _service = service; }

        public IReadOnlyList<digioz.Portal.Bo.Config> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Config>();
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, pageSize));

        public void OnGet()
        {
            var all = _service.GetAll().OrderBy(c => c.ConfigKey).ToList();
            TotalCount = all.Count;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;
            Items = all.Skip(skip).Take(pageSize).ToList();
        }

        public string DisplayValue(digioz.Portal.Bo.Config cfg)
        {
            if (cfg.IsEncrypted) return "(encrypted)"; // placeholder
            return cfg.ConfigValue ?? string.Empty;
        }
    }
}
