using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;

namespace digioz.Portal.Web.Middleware
{
    /// <summary>
    /// Middleware to disable request size limits and timeouts for chunked upload endpoints
    /// </summary>
    public class ChunkedUploadMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ChunkedUploadMiddleware> _logger;

        public ChunkedUploadMiddleware(RequestDelegate next, ILogger<ChunkedUploadMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is a chunked upload endpoint
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            
            if (path.Contains("/api/chunkedupload"))
            {
                _logger.LogInformation($"ChunkedUpload request detected: {path}");
                
                // Get the IHttpMaxRequestBodySizeFeature to override request size limits
                var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (maxRequestBodySizeFeature != null)
                {
                    // Disable the max request body size limit for this request
                    maxRequestBodySizeFeature.MaxRequestBodySize = null;
                    _logger.LogInformation("Disabled max request body size limit for chunked upload");
                }

                // Disable the minimum data rate to allow slow uploads
                var minRequestBodyDataRateFeature = context.Features.Get<IHttpMinRequestBodyDataRateFeature>();
                if (minRequestBodyDataRateFeature != null)
                {
                    minRequestBodyDataRateFeature.MinDataRate = null;
                    _logger.LogInformation("Disabled minimum data rate for chunked upload");
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method to register the middleware
    /// </summary>
    public static class ChunkedUploadMiddlewareExtensions
    {
        public static IApplicationBuilder UseChunkedUploadMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ChunkedUploadMiddleware>();
        }
    }
}
