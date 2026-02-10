using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class OrderDetail
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string OrderId { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Description { get; set; }
        [MaxLength(50)]
        public string Size { get; set; }
        [MaxLength(50)]
        public string Color { get; set; }
        [MaxLength(50)]
        public string MaterialType { get; set; }
        public string Notes { get; set; }
    }
}
