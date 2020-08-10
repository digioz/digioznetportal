using System;
using System.Collections.Generic;

namespace digiozPortal.BO
{
    public partial class Plugin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dll { get; set; }
        public bool IsEnabled { get; set; }
    }
}
