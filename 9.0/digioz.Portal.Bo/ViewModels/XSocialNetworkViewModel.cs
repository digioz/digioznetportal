using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Bo.ViewModels
{
    public class XSocialNetworkViewModel
    {
        public XSocialNetworkViewModel(string twitterHandle, bool enabled)
        {
            Id = Guid.NewGuid();
            XHandle = twitterHandle;
            XUser = XHandle.Replace("@", "");
            Enabled = enabled;
        }
        public Guid Id { get; set; }
        public string XHandle { get; set; }
        public string XUser { get; set; }
        public bool Enabled { get; set; }
    }
}
