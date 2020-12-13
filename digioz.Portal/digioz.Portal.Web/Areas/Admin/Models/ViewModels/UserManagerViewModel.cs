using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Web.Areas.Admin.Models.ViewModels
{
    public class UserManagerViewModel
    {
        public UserManagerViewModel() {
            Countries = Utility.CCGetCountryList();
        }

        public string Id { get; set; }
        public int ProfileID { get; set; }
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string PasswordConfirm { get; set; }

        public string Email { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        [Display(Name = "Birthday Visible")]
        public Nullable<bool> BirthdayVisible { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Signature { get; set; }
        public string Avatar { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile AvatarImage { get; set; }

        public bool UseGravatar { get; set; }
        public List<SelectListItem> Countries { get; set; }
    }
}
