using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Product
    {
        public string Id { get; set; }
        public string ProductCategoryId { get; set; }
        public string Name { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal? Cost { get; set; }
        public int? QuantityPerUnit { get; set; }
        public string Weight { get; set; }
        public string Dimensions { get; set; }
        public string Sizes { get; set; }
        public string Colors { get; set; }
        public string MaterialType { get; set; }
        public string PartNumber { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string ManufacturerUrl { get; set; }
        public int? UnitsInStock { get; set; }
        public bool OutOfStock { get; set; }
        public string Notes { get; set; }
        public bool Visible { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
