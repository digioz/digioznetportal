using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digioz.Portal.Bo.ViewModels
{
    public class Twitter
    {
        public Twitter(string twitterHandle, bool enabled)
        {
            Id = Guid.NewGuid();
            TwitterHandle = twitterHandle;
            TwitterUser = TwitterHandle.Replace("@", "");
            Enabled = enabled;
        }
        public Guid Id { get; set; }
        public string TwitterHandle { get; set; }
        public string TwitterUser { get; set; }
        public bool Enabled { get; set; }
    }
}
