using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.UserManager
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly IAspNetUserService _userService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(IAspNetUserService userService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _userService = userService;
            _profileService = profileService;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel? Input { get; set; }

        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Id { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string? Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }

            // Profile fields
            [Display(Name = "Display Name")]
            public string? DisplayName { get; set; }

            [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
            [Display(Name = "First Name")]
            public string? FirstName { get; set; }

            [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters.")]
            [Display(Name = "Middle Name")]
            public string? MiddleName { get; set; }

            [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
            [Display(Name = "Last Name")]
            public string? LastName { get; set; }

            [Display(Name = "Birthday")]
            [DataType(DataType.Date)]
            public DateTime? Birthday { get; set; }

            [Display(Name = "Birthday Visible")]
            public bool BirthdayVisible { get; set; }

            public string? Address { get; set; }

            [Display(Name = "Address 2")]
            public string? Address2 { get; set; }

            [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
            public string? City { get; set; }

            [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
            public string? State { get; set; }

            [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters.")]
            public string? Zip { get; set; }

            [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters.")]
            public string? Country { get; set; }

            [StringLength(255, ErrorMessage = "Signature cannot exceed 255 characters.")]
            public string? Signature { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = _userService.Get(id);
            if (user == null)
            {
                return NotFound();
            }

            var profile = _profileService.GetByUserId(id);

            Input = new InputModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                DisplayName = profile?.DisplayName,
                FirstName = profile?.FirstName,
                MiddleName = profile?.MiddleName,
                LastName = profile?.LastName,
                Birthday = profile?.Birthday,
                BirthdayVisible = profile?.BirthdayVisible ?? false,
                Address = profile?.Address,
                Address2 = profile?.Address2,
                City = profile?.City,
                State = profile?.State,
                Zip = profile?.Zip,
                Country = profile?.Country,
                Signature = profile?.Signature
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || Input == null)
            {
                return Page();
            }

            var identityUser = await _userManager.FindByIdAsync(Input.Id);
            if (identityUser == null)
            {
                return NotFound();
            }

            // Update username and email
            identityUser.UserName = Input.UserName;
            identityUser.Email = Input.Email;

            // Update password if provided
            if (!string.IsNullOrEmpty(Input.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                var result = await _userManager.ResetPasswordAsync(identityUser, token, Input.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            var updateResult = await _userManager.UpdateAsync(identityUser);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Update or create profile
            var profile = _profileService.GetByUserId(Input.Id);
            if (profile == null)
            {
                profile = new Profile
                {
                    UserId = Input.Id
                };
            }

            profile.DisplayName = Input.DisplayName;
            profile.Email = Input.Email;
            profile.FirstName = Input.FirstName;
            profile.MiddleName = Input.MiddleName;
            profile.LastName = Input.LastName;
            profile.Birthday = Input.Birthday;
            profile.BirthdayVisible = Input.BirthdayVisible;
            profile.Address = Input.Address;
            profile.Address2 = Input.Address2;
            profile.City = Input.City;
            profile.State = Input.State;
            profile.Zip = Input.Zip;
            profile.Country = Input.Country;
            profile.Signature = Input.Signature;

            if (profile.Id == 0)
            {
                _profileService.Add(profile);
            }
            else
            {
                _profileService.Update(profile);
            }

            StatusMessage = "User updated successfully.";
            return RedirectToPage("/UserManager/Details", new { id = Input.Id });
        }
    }
}
