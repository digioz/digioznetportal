using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class DeleteModel : PageModel
    {
        private readonly IZoneService _zoneService;
        public DeleteModel(IZoneService zoneService) { _zoneService = zoneService; }

        [BindProperty]
        public int Id { get; set; }

        public IActionResult OnGet(int id)
        {
            Id = id;
            return Page();
        }

        public IActionResult OnPost()
        {
            _zoneService.Delete(Id);
            return RedirectToPage("/Zone/Index");
        }
    }
}
