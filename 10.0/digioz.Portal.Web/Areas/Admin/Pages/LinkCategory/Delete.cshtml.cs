using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.LinkCategory
{
    public class DeleteModel : PageModel
    {
        private readonly ILinkCategoryService _service;
  
        public DeleteModel(ILinkCategoryService service)
        {
            _service = service;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public digioz.Portal.Bo.LinkCategory? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
      if (Item == null)
            {
      return RedirectToPage("/LinkCategory/Index", new { area = "Admin" });
            }
    return Page();
        }

    public IActionResult OnPost()
        {
      _service.Delete(Id);
   return RedirectToPage("/LinkCategory/Index", new { area = "Admin" });
   }
    }
}
