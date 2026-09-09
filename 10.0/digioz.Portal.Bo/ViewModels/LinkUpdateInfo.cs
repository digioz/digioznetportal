namespace digioz.Portal.Bo.ViewModels
{
    /// <summary>
    /// Contains link update information captured during link checking to avoid race conditions
    /// </summary>
    public class LinkUpdateInfo
    {
        /// <summary>
        /// The ID of the link to update
        /// </summary>
        public int LinkId { get; set; }

        /// <summary>
        /// Whether the link should be visible
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// The updated description for the link
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
