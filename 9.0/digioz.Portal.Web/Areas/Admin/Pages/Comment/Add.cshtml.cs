using System;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Comment
{
    public class AddModel : PageModel
    {
        private readonly ICommentService _service;
        public AddModel(ICommentService service) { _service = service; }

        [BindProperty]
        public Bo.Comment Item { get; set; } = new Bo.Comment();

        public void OnGet()
        {
            // Initialize with default values
            Item.Visible = true;
            Item.Approved = true;
            Item.Likes = 0;
            Item.CreatedDate = DateTime.UtcNow;
            Item.ModifiedDate = DateTime.UtcNow;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            
            Item.Id = Guid.NewGuid().ToString();
            Item.CreatedDate = DateTime.UtcNow;
            Item.ModifiedDate = DateTime.UtcNow;
            Item.Likes = 0;
            
            _service.Add(Item);
            return RedirectToPage("/Comment/Index", new { area = "Admin" });
        }
    }
}
