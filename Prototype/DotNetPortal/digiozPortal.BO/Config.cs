using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Config
    {
        public int Id { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsEncrypted { get; set; }
    }
}
