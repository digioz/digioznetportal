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
    public class AddModel : PageModel
    {
        private readonly ICommentService _service;
        private readonly UserManager<IdentityUser> _userManager;
        
        public AddModel(ICommentService service, UserManager<IdentityUser> userManager) 
        { 
            _service = service;
            _userManager = userManager;
        }

        [BindProperty]
        public Bo.Comment Item { get; set; } = new Bo.Comment();
        
        [BindProperty]
        public bool VisibleCheckbox { get; set; }
        
        [BindProperty]
        public bool ApprovedCheckbox { get; set; }
        
        public SelectList UserList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public async Task OnGetAsync()
        {
            // Initialize with default values
            Item.Likes = 0;
            Item.CreatedDate = DateTime.UtcNow;
            Item.ModifiedDate = DateTime.UtcNow;
            VisibleCheckbox = false;
            ApprovedCheckbox = false;
            
            // Load users for dropdown
            await LoadUsersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                return Page();
            }
            
            Item.Id = Guid.NewGuid().ToString();
            Item.CreatedDate = DateTime.UtcNow;
            Item.ModifiedDate = DateTime.UtcNow;
            Item.Likes = 0;
            Item.Visible = VisibleCheckbox;
            Item.Approved = ApprovedCheckbox;
            
            // Get username from UserId
            if (!string.IsNullOrEmpty(Item.UserId))
            {
                var user = await _userManager.FindByIdAsync(Item.UserId);
                Item.Username = user?.UserName;
            }
            
            _service.Add(Item);
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
                
            UserList = new SelectList(users, "Value", "Text");
        }
    }
}
