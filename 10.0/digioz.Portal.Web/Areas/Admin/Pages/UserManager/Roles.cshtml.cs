using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.UserManager
{
    [Authorize(Roles = "Administrator")]
    public class RolesModel : PageModel
    {
        private readonly IAspNetUserService _userService;
        private readonly IAspNetRoleService _roleService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesModel(
            IAspNetUserService userService,
            IAspNetRoleService roleService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _roleService = roleService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public new AspNetUser? User { get; set; }
        public List<RoleInfo> UserRoles { get; set; } = new();
        public List<SelectListItem> AvailableRoles { get; set; } = new();

        [BindProperty]
        public string? SelectedRoleId { get; set; }

        public string? StatusMessage { get; set; }

        public class RoleInfo
        {
            public required string RoleId { get; set; }
            public required string RoleName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User = _userService.Get(id);
            if (User == null)
            {
                return NotFound();
            }

            await LoadRolesAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string id)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(SelectedRoleId))
            {
                return NotFound();
            }

            User = _userService.Get(id);
            if (User == null)
            {
                return NotFound();
            }

            var identityUser = await _userManager.FindByIdAsync(id);
            var role = await _roleManager.FindByIdAsync(SelectedRoleId);

            if (identityUser == null || role == null)
            {
                StatusMessage = "Error: User or role not found.";
                await LoadRolesAsync(id);
                return Page();
            }

            var result = await _userManager.AddToRoleAsync(identityUser, role.Name ?? string.Empty);
            if (result.Succeeded)
            {
                StatusMessage = $"Role '{role.Name}' added successfully.";
            }
            else
            {
                StatusMessage = $"Error adding role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            await LoadRolesAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string id, string roleId)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(roleId))
            {
                return NotFound();
            }

            User = _userService.Get(id);
            if (User == null)
            {
                return NotFound();
            }

            var identityUser = await _userManager.FindByIdAsync(id);
            var role = await _roleManager.FindByIdAsync(roleId);

            if (identityUser == null || role == null)
            {
                StatusMessage = "Error: User or role not found.";
                await LoadRolesAsync(id);
                return Page();
            }

            var result = await _userManager.RemoveFromRoleAsync(identityUser, role.Name ?? string.Empty);
            if (result.Succeeded)
            {
                StatusMessage = $"Role '{role.Name}' removed successfully.";
            }
            else
            {
                StatusMessage = $"Error removing role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            await LoadRolesAsync(id);
            return Page();
        }

        private async Task LoadRolesAsync(string userId)
        {
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser != null)
            {
                var userRoleNames = await _userManager.GetRolesAsync(identityUser);
                var allRoles = _roleService.GetAll();

                UserRoles = allRoles
                    .Where(r => userRoleNames.Contains(r.Name))
                    .Select(r => new RoleInfo
                    {
                        RoleId = r.Id,
                        RoleName = r.Name ?? string.Empty
                    })
                    .OrderBy(r => r.RoleName)
                    .ToList();

                // Get roles not assigned to user
                var availableRolesList = allRoles
                    .Where(r => !userRoleNames.Contains(r.Name))
                    .OrderBy(r => r.Name)
                    .ToList();

                AvailableRoles = availableRolesList.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name ?? string.Empty
                }).ToList();
            }
        }
    }
}
