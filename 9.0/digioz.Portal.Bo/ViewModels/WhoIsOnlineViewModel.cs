using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digioz.Portal.Bo.ViewModels
{
    public class WhoIsOnlineViewModel
    {
        public int VisitorCount { get; set; }
        public List<VisitorSession> RegisteredVisitors { get; set; } = new List<VisitorSession>();
        public bool WhoIsOnlineEnabled { get; set; } = false;
    }
}
