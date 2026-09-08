namespace digioz.Portal.EmailProviders.Models.Configuration;

/// <summary>
/// Azure Communication Services Email provider settings
/// </summary>
public class AzureEmailSettings
{
    /// <summary>
    /// Azure Communication Services connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Azure Communication Services endpoint
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Optional: Access key
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Enable user engagement tracking
    /// </summary>
    public bool EnableTracking { get; set; } = true;
}
