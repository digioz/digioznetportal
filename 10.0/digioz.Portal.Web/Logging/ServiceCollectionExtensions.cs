using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace digioz.Portal.Web.Logging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVisitorInfoLogging(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            // Add filter globally via MvcOptions.Filters to ensure it's applied to Razor Pages
            services.AddMvc(options => options.Filters.Add<VisitorLoggingPageFilter>());
            services.AddScoped<VisitorLoggingPageFilter>();
            services.AddSingleton<IVisitorInfoQueue, VisitorInfoQueue>();
            services.AddHostedService<VisitorInfoBackgroundService>();
            return services;
        }
    }
}
