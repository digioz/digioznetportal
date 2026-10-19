using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class VisitorSession
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string IpAddress { get; set; }
        public string PageUrl { get; set; }
        public string SessionId { get; set; }
        [MaxLength(255)]
        public string Username { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        
        // Navigation property
        public virtual Profile Profile { get; set; }
    }
}
