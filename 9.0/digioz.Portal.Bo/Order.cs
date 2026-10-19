using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Order
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        [MaxLength(20)]
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(70)]
        public string ShippingAddress { get; set; }
        [MaxLength(70)]
        public string ShippingAddress2 { get; set; }
        [MaxLength(40)]
        public string ShippingCity { get; set; }
        [MaxLength(40)]
        public string ShippingState { get; set; }
        [MaxLength(30)]
        public string ShippingZip { get; set; }
        [MaxLength(50)]
        public string ShippingCountry { get; set; }
        [MaxLength(70)]
        public string BillingAddress { get; set; }
        [MaxLength(70)]
        public string BillingAddress2 { get; set; }
        [MaxLength(40)]
        public string BillingCity { get; set; }
        [MaxLength(40)]
        public string BillingState { get; set; }
        [MaxLength(30)]
        public string BillingZip { get; set; }
        [MaxLength(50)]
        public string BillingCountry { get; set; }
        [MaxLength(30)]
        public string Phone { get; set; }
        [MaxLength(255)]
        public string Email { get; set; }
        public decimal Total { get; set; }
        [MaxLength(100)]
        public string Ccnumber { get; set; }
        [MaxLength(10)]
        public string Ccexp { get; set; }
        [MaxLength(10)]
        public string CccardCode { get; set; }
        public decimal Ccamount { get; set; }
        public string TrxDescription { get; set; }
        public bool TrxApproved { get; set; }
        [MaxLength(100)]
        public string TrxAuthorizationCode { get; set; }
        public string TrxMessage { get; set; }
        [MaxLength(10)]
        public string TrxResponseCode { get; set; }
        public string TrxId { get; set; }
    }
}
