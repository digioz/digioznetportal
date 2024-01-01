using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace digiozPortal.BO
{
    [Table("Log")]
    public partial class Log
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
