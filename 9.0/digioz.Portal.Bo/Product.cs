using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Product
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string ProductCategoryId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Make { get; set; }
        [MaxLength(50)]
        public string Model { get; set; }
        [MaxLength(50)]
        public string Sku { get; set; }
        [MaxLength(50)]
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal? Cost { get; set; }
        public int? QuantityPerUnit { get; set; }
        [MaxLength(20)]
        public string Weight { get; set; }
        [MaxLength(50)]
        public string Dimensions { get; set; }
        [MaxLength(50)]
        public string Sizes { get; set; }
        [MaxLength(50)]
        public string Colors { get; set; }
        [MaxLength(50)]
        public string MaterialType { get; set; }
        [MaxLength(50)]
        public string PartNumber { get; set; }
        [MaxLength(255)]
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string ManufacturerUrl { get; set; }
        public int? UnitsInStock { get; set; }
        public bool OutOfStock { get; set; }
        public string Notes { get; set; }
        public bool Visible { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int Views { get; set; }
    }
}
