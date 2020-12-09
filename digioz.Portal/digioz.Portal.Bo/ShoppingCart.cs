using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class ShoppingCart
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool? Size { get; set; }
        public string Color { get; set; }
        public string MaterialType { get; set; }
        public string Notes { get; set; }
    }
}
