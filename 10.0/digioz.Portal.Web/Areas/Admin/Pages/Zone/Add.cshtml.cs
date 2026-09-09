using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Web.Pages.Shared.Components.ZoneMenu;
using Microsoft.Extensions.Caching.Memory;
using ZoneEntity = digioz.Portal.Bo.Zone;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class AddModel : PageModel
    {
        private readonly IZoneService _zoneService;
        private readonly IMemoryCache _cache;
        public AddModel(IZoneService zoneService, IMemoryCache cache) { _zoneService = zoneService; _cache = cache; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList LocationOptions => new SelectList(Locations);

        public class InputModel
        {
            [Required]
            public string Name { get; set; } = string.Empty;
            [Required]
            [Display(Name = "Location")]
            public string Location { get; set; } = string.Empty;
            [Display(Name = "Content")]
            public string Body { get; set; } = string.Empty;
            public bool Visible { get; set; } = true;
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            var zone = new ZoneEntity
            {
                Name = InputSanitizer.SanitizeText(Input.Name),
                Location = Input.Location,
                Body = Input.Body,
                Visible = Input.Visible,
                Timestamp = DateTime.UtcNow
            };
            _zoneService.Add(zone);
            _cache.Remove(ZoneMenuViewComponent.CacheKey);
            return RedirectToPage("/Zone/Index");
        }

        public static readonly string[] Locations = new[] { "Top", "TopMenu", "Left", "LeftMenu", "BodyTop", "BodyBottom", "Bottom" };
    }
}
