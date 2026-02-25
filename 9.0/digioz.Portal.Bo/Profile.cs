using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Profile
    {
        public int Id { get; set; }
        [StringLength(128)]
        public string UserId { get; set; }
        [StringLength(50)]
        public string DisplayName { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(50)]
        public string MiddleName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(255)]
        public string Email { get; set; }
        public DateTime? Birthday { get; set; }
        public bool? BirthdayVisible { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        [StringLength(50)]
        public string City { get; set; }
        [StringLength(50)]
        public string State { get; set; }
        [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters.")]
        public string Zip { get; set; }
        [StringLength(50)]
        public string Country { get; set; }
        [StringLength(255)]
        public string Signature { get; set; }
        [StringLength(50)]
        public string Avatar { get; set; }
        public int? ThemeId { get; set; }
        public int Views { get; set; }
        
        // Navigation property
        public virtual Theme Theme { get; set; }
    }
}
