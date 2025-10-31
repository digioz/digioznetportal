using System;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Comment
{
    public class EditModel : PageModel
    {
        private readonly ICommentService _service;
        public EditModel(ICommentService service) { _service = service; }

        [BindProperty]
        public Bo.Comment Item { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            // Load the existing record and only update allowed fields
            var existing = _service.Get(Item.Id);
            if (existing == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            existing.Body = Item.Body; // only editable field
            existing.ModifiedDate = DateTime.UtcNow; // update modification time
            _service.Update(existing);
            return RedirectToPage("/Comment/Index", new { area = "Admin" });
        }
    }
}
