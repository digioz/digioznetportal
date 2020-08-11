using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("ProductOption")]
    public partial class ProductOption
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string OptionType { get; set; }
        public string OptionValue { get; set; }
    }
}
