using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("ProductCategory")]
    public partial class ProductCategory
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public string Description { get; set; }
    }
}
