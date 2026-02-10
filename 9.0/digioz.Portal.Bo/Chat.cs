using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Chat
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
