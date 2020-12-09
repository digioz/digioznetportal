using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Plugin")]
    public partial class Plugin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dll { get; set; }
        public bool IsEnabled { get; set; }
    }
}
