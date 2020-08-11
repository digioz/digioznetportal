using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Zone")]
    public partial class Zone
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ZoneType { get; set; }
    }
}
