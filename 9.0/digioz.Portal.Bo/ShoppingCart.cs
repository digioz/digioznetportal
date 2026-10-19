using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class ShoppingCart
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public string Size { get; set; }
        [MaxLength(50)]
        public string Color { get; set; }
        [MaxLength(50)]
        public string MaterialType { get; set; }
        public string Notes { get; set; }
    }
}
