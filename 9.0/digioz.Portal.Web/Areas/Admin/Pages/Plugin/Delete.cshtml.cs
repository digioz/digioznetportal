using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Plugin
{
 public class DeleteModel : PageModel
 {
 private readonly IPluginService _service;
 public DeleteModel(IPluginService service) { _service = service; }

 [BindProperty(SupportsGet = true)] public int Id { get; set; }
 public Bo.Plugin? Item { get; private set; }

 public IActionResult OnGet(int id)
 {
 Item = _service.Get(id);
 if (Item == null) return RedirectToPage("/Plugin/Index", new { area = "Admin" });
 return Page();
 }

 public IActionResult OnPost()
 {
 _service.Delete(Id);
 return RedirectToPage("/Plugin/Index", new { area = "Admin" });
 }
 }
}
