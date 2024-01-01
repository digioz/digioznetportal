using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Web.Helpers;

namespace digioz.Portal.Web.Models
{
    public class Twitter
    {
        public Twitter(string twitterHandle, bool enabled) {
            Id = GuidComb.GenerateComb();
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
