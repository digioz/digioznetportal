using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Menu
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public bool Visible { get; set; }
        public DateTime? Timestamp { get; set; }
        public int SortOrder { get; set; }
    }
}
