using digioz.Portal.EmailProviders.Models;
using digioz.Portal.EmailProviders.Models.Configuration;

namespace digioz.Portal.EmailProviders.Interfaces;

/// <summary>
/// Factory for creating email provider instances based on configuration
/// </summary>
public interface IEmailProviderFactory
{
    /// <summary>
    /// Creates an email provider instance based on the configuration
    /// </summary>
    IEmailProvider CreateProvider(EmailConfiguration configuration);

    /// <summary>
    /// Gets a list of all supported provider types
    /// </summary>
    IEnumerable<string> GetSupportedProviders();
}
