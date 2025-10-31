using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Module
{
    public class AddModel : PageModel
    {
        private readonly IModuleService _service;
        public AddModel(IModuleService service) { _service = service; }

        [BindProperty]
        public Bo.Module Item { get; set; } = new Bo.Module { Visible = true, DisplayInBox = true };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            _service.Add(Item);
            return RedirectToPage("/Module/Index", new { area = "Admin" });
        }
    }
}
