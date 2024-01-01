using System;
using System.Collections.Generic;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class VisitorSession
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string PageUrl { get; set; }
        public string SessionId { get; set; }
        public string Username { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
