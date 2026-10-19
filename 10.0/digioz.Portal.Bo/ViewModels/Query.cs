using System.Collections.Generic;

namespace digioz.Portal.Bo.ViewModels
{
    public class Query
    {
        public int Top { get; set; }
        public string Select { get; set; }
        public string Fields { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
    }
}
