using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace digioz.Portal.Bo.ViewModels
{
    public class ProfileViewModel
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Username { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        [DisplayName("Birthday Visible")]
        public Nullable<bool> BirthdayVisible { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Signature { get; set; }
        public string Avatar { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        public bool UseGravatar { get; set; }

        public IFormFile AvatarImage { get; set; }
        public List<string> Countries { get; set; }

        public ProfileViewModel()
        {
            Countries = new List<string>();
        }
        public ProfileViewModel(List<string> countries)
        {
            Countries = countries;
        }
    }
}