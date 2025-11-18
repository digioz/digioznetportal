using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using digioz.Portal.Dal.Services.Interfaces;
using ZoneEntity = digioz.Portal.Bo.Zone;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class AddModel : PageModel
    {
        private readonly IZoneService _zoneService;
        public AddModel(IZoneService zoneService) { _zoneService = zoneService; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList LocationOptions => new SelectList(Locations);

        public class InputModel
        {
            [Required]
            public string Name { get; set; }
            [Required]
            [Display(Name = "Location")]
            public string Location { get; set; }
            [Display(Name = "Content")]
            public string Body { get; set; }
            public bool Visible { get; set; } = true;
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            var zone = new ZoneEntity
            {
                Name = Input.Name,
                Location = Input.Location,
                Body = Input.Body,
                Visible = Input.Visible,
                Timestamp = DateTime.UtcNow
            };
            _zoneService.Add(zone);
            return RedirectToPage("/Zone/Index");
        }

        public static readonly string[] Locations = new[] { "Top", "TopMenu", "Left", "LeftMenu", "BodyTop", "BodyBottom", "Bottom" };
    }
}
