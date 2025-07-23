using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class ProductOption
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string OptionType { get; set; }
        public string OptionValue { get; set; }
    }
}
