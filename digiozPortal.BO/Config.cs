using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
 
    [Table("Config")]
    public partial class Config
    {
        public int Id { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
