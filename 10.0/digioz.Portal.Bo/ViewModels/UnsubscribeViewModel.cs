using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace digioz.Portal.Bo.ViewModels
{
    public class UnsubscribeViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public bool Unsubscribe { get; set; }
    }
}