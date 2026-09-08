using System;

namespace digioz.Portal.Bo.ViewModels
{
    /// <summary>
    /// Represents the result of checking a single link
    /// </summary>
    public class LinkCheckResult
    {
        public int LinkId { get; set; }
        public string LinkName { get; set; }
        public string Url { get; set; }
        public LinkCheckStatus Status { get; set; }
        public int? HttpStatusCode { get; set; }
        public string Message { get; set; }
        public bool WasUpdated { get; set; }
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// Status of a link check operation
    /// </summary>
    public enum LinkCheckStatus
    {
        Success,
        DeadLink,
        ErrorLink,
        RedirectLink,
        DescriptionUpdated,
        NetworkError,
        Timeout
    }
}
