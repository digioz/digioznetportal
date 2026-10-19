using digioz.Portal.EmailProviders.Interfaces;
using digioz.Portal.EmailProviders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace digioz.Portal.EmailProviders.Extensions;

/// <summary>
/// Extension methods for registering email provider services with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds email provider services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailProviders(this IServiceCollection services)
    {
        // Register the factory as singleton since it's stateless
        services.AddSingleton<IEmailProviderFactory, EmailProviderFactory>();

        // Register the email service as scoped (typical for services that may use scoped dependencies in the future)
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    /// <summary>
    /// Adds email provider services to the dependency injection container with transient lifetime
    /// Use this if you need a new instance for each request
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailProvidersTransient(this IServiceCollection services)
    {
        services.AddSingleton<IEmailProviderFactory, EmailProviderFactory>();
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }

    /// <summary>
    /// Adds email provider services to the dependency injection container with singleton lifetime
    /// Use this if you want to reuse the same instance across the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEmailProvidersSingleton(this IServiceCollection services)
    {
        services.AddSingleton<IEmailProviderFactory, EmailProviderFactory>();
        services.AddSingleton<IEmailService, EmailService>();

        return services;
    }
}
