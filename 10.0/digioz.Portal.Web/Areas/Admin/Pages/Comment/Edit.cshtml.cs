using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace digioz.Portal.Web.Areas.Admin.Pages.Comment
{
    public class EditModel : PageModel
    {
        private readonly ICommentService _service;
        private readonly UserManager<IdentityUser> _userManager;
        
        public EditModel(ICommentService service, UserManager<IdentityUser> userManager) 
        { 
            _service = service;
            _userManager = userManager;
        }

        [BindProperty]
        public Bo.Comment? Item { get; set; }
        
        [BindProperty]
        public bool VisibleCheckbox { get; set; }
        
        [BindProperty]
        public bool ApprovedCheckbox { get; set; }
        
        public SelectList UserList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            
            // Map nullable bool? to non-nullable bool for display
            VisibleCheckbox = Item.Visible == true;
            ApprovedCheckbox = Item.Approved == true;
            
            // Load users for dropdown
            await LoadUsersAsync();
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                return Page();
            }
            
            if (Item == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            
            var existing = _service.Get(Item.Id);
            if (existing == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            
            existing.Body = Item.Body;
            existing.UserId = Item.UserId;
            
            // Get username from UserId
            if (!string.IsNullOrEmpty(Item.UserId))
            {
                var user = await _userManager.FindByIdAsync(Item.UserId);
                existing.Username = user?.UserName;
            }
            
            // Map non-nullable bool back to nullable bool?
            existing.Visible = VisibleCheckbox;
            existing.Approved = ApprovedCheckbox;
            existing.ModifiedDate = DateTime.UtcNow;
            
            _service.Update(existing);
            return RedirectToPage("/Comment/Index", new { area = "Admin" });
        }
        
        private async Task LoadUsersAsync()
        {
            var users = _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                })
                .ToList();
                
            UserList = new SelectList(users, "Value", "Text", Item?.UserId);
        }
    }
}
