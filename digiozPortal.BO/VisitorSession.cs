using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("VisitorSession")]
    public partial class VisitorSession
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string PageUrl { get; set; }
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
