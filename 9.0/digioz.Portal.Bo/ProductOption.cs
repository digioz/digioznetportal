using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class ProductOption
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        [MaxLength(50)]
        public string OptionType { get; set; }
        [MaxLength(50)]
        public string OptionValue { get; set; }
    }
}
