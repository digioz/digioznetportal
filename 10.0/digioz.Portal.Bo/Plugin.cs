using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Plugin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Dll { get; set; }
        public bool IsEnabled { get; set; }
    }
}
