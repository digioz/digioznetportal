#nullable enable

using System;

namespace digioz.Portal.Bo
{
    public partial class PrivateMessage
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string FromId { get; set; } = string.Empty;
        public string FromIp { get; set; } = string.Empty;
        public string ToId { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public bool Reported { get; set; }
    }
}
