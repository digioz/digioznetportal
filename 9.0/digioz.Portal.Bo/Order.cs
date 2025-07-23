using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Order
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingCountry { get; set; }
        public string BillingAddress { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string BillingCountry { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal Total { get; set; }
        public string Ccnumber { get; set; }
        public string Ccexp { get; set; }
        public string CccardCode { get; set; }
        public decimal Ccamount { get; set; }
        public string TrxDescription { get; set; }
        public bool TrxApproved { get; set; }
        public string TrxAuthorizationCode { get; set; }
        public string TrxMessage { get; set; }
        public string TrxResponseCode { get; set; }
        public string TrxId { get; set; }
    }
}
