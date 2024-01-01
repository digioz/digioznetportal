using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using digioz.Portal.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using digioz.Portal.Payment;

namespace digioz.Portal.Web.Models.ViewModels
{
    public class CheckOutViewModel
    {
        public CheckOutViewModel()
        {
            BillingCountries = Utility.CCGetCountryList();
            ShippingCountries = Utility.CCGetCountryList();
            MonthList = Utility.CCGetMonthList();
            YearsList = Utility.CCGetYearList();
            PaymentGatewayList = Utility.GetPaymentGateways();
        }

        [Required]
        [StringLength(50)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(70)]
        [DisplayName("Shipping Address")]
        public string ShippingAddress { get; set; }

        [StringLength(70)]
        [DisplayName("Shipping Address 2")]
        public string ShippingAddress2 { get; set; }

        [Required]
        [StringLength(40)]
        [DisplayName("Shipping City")]
        public string ShippingCity { get; set; }

        [StringLength(40)]
        [DisplayName("Shipping State")]
        public string ShippingState { get; set; }

        [Required]
        [StringLength(30)]
        [DisplayName("Shipping Zip")]
        public string ShippingZip { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Shipping Country")]
        public string ShippingCountry { get; set; }

        [Required]
        [StringLength(70)]
        [DisplayName("Billing Address")]
        public string BillingAddress { get; set; }

        [StringLength(70)]
        [DisplayName("Billing Address 2")]
        public string BillingAddress2 { get; set; }

        [Required]
        [StringLength(40)]
        [DisplayName("Billing City")]
        public string BillingCity { get; set; }

        [Required]
        [StringLength(40)]
        [DisplayName("Billing State")]
        public string BillingState { get; set; }

        [Required]
        [StringLength(30)]
        [DisplayName("Billing Zip")]
        public string BillingZip { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Billing Country")]
        public string BillingCountry { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [Required]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Credit Card Number")]
        public string CCNumber { get; set; }

        [Required]
        [StringLength(10)]
        [DisplayName("Expiration Month")]
        public string CCExpMonth { get; set; }

        [Required]
        [StringLength(10)]
        [DisplayName("Expiration Year")]
        public string CCExpYear { get; set; }

        [Required]
        [StringLength(10)]
        [DisplayName("Security Code")]
        public string CCCardCode { get; set; }

        [Required]
        [StringLength(40)]
        [DisplayName("Payment Gateway")]
        public string PaymentGateway { get; set; }


        public List<SelectListItem> BillingCountries { get; set; }
        public List<SelectListItem> ShippingCountries { get; set; }
        public List<SelectListItem> MonthList { get; set; }
        public List<SelectListItem> YearsList { get; set; }
        public List<SelectListItem> PaymentGatewayList { get; set; }
    }
}