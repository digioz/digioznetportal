#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace digioz.Portal.Bo
{
    public partial class PrivateMessage
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        [MaxLength(128)]
        public string FromId { get; set; } = string.Empty;
        [MaxLength(64)]
        public string FromIp { get; set; } = string.Empty;
        [MaxLength(128)]
        public string ToId { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        [MaxLength(255)]
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public bool Reported { get; set; }
    }
}
