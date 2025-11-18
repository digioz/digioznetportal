using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using digioz.Portal.Dal.Services.Interfaces;
using ZoneEntity = digioz.Portal.Bo.Zone;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class EditModel : PageModel
    {
        private readonly IZoneService _zoneService;
        public EditModel(IZoneService zoneService) { _zoneService = zoneService; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList LocationOptions => new SelectList(Locations);

        public class InputModel
        {
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            [Display(Name = "Location")]
            public string Location { get; set; }
            [Display(Name = "Content")]
            public string Body { get; set; }
            public bool Visible { get; set; }
        }

        public IActionResult OnGet(int id)
        {
            var zone = _zoneService.Get(id);
            if (zone == null) return RedirectToPage("/Zone/Index");
            Input = new InputModel
            {
                Id = zone.Id,
                Name = zone.Name,
                Location = zone.Location,
                Body = zone.Body,
                Visible = zone.Visible
            };
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            var zone = _zoneService.Get(Input.Id);
            if (zone == null) return RedirectToPage("/Zone/Index");

            zone.Name = Input.Name;
            zone.Location = Input.Location;
            zone.Body = Input.Body;
            zone.Visible = Input.Visible;
            zone.Timestamp = DateTime.UtcNow;

            _zoneService.Update(zone);
            return RedirectToPage("/Zone/Index");
        }

        public static readonly string[] Locations = new[] { "Top", "TopMenu", "Left", "LeftMenu", "BodyTop", "BodyBottom", "Bottom" };
    }
}
