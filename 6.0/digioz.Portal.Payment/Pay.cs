using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Payment
{
    public class Pay
    {
        public string ID { get; set; }
        public DateTime OrderDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingCountryCode { get; set; }
        public string BillingAddress { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string BillingCountry { get; set; }
        public string BillingCountryCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal Total { get; set; }
        public string CCNumber { get; set; }
        public string CCExp { get; set; }

        public string CCExpMonth { get; set; }
        public string CCExpYear { get; set; }
        public string CCCardCode { get; set; }
        public decimal? CCAmount { get; set; }

        public string CCType { get; set; }
        public string Description { get; set; }
    }
}
