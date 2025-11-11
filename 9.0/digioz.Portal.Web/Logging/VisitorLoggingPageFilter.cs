using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Logging
{
    public sealed class VisitorLoggingPageFilter : IAsyncPageFilter
    {
        private const string HandlerItemKey = "__Visitor.Handler__";
        private readonly IVisitorInfoQueue _queue;
        private readonly IVisitorSessionService _visitorSessionService;

        public VisitorLoggingPageFilter(IVisitorInfoQueue queue, IVisitorSessionService visitorSessionService)
        {
            _queue = queue;
            _visitorSessionService = visitorSessionService;
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            var handlerName = context.HandlerMethod?.MethodInfo?.Name;
            context.HttpContext.Items[HandlerItemKey] = handlerName;
            return Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var sw = Stopwatch.StartNew();
            var executedContext = await next();
            sw.Stop();

            var http = context.HttpContext;
            var req = http.Request;
            var route = context.RouteData.Values;

            string? pagePath = null;
            string? area = null;
            string? handler = null;
            if (context.ActionDescriptor is PageActionDescriptor pad)
            {
                pagePath = pad.ViewEnginePath;
                area = pad.AreaName;
            }
            handler = (route.TryGetValue("handler", out var h) ? h?.ToString() : null) ?? http.Items[HandlerItemKey] as string;

            var (userId, userName, isAuth) = GetUser(http);
            var ip = GetClientIp(http);
            var referrer = req.Headers["Referer"].ToString();
            var userAgent = req.Headers["User-Agent"].ToString();
            var url = $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}";
            var sessionId = http.Session?.IsAvailable == true ? http.Session.Id : null;

            // Map to existing BO VisitorInfo fields
            var info = new VisitorInfo
            {
                Timestamp = DateTime.UtcNow,
                Host = req.Host.HasValue ? req.Host.Value : null,
                HostName = req.Host.Host,
                IpAddress = ip,
                Platform = CultureInfo.CurrentCulture?.Name,
                Referrer = referrer,
                Href = url,
                UserAgent = userAgent,
                UserLanguage = GetAcceptedLanguage(req),
                SessionId = sessionId
            };

            _queue.TryEnqueue(info);

            // Update or create VisitorSession record
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var existingSession = _visitorSessionService.GetAll()
                        .FirstOrDefault(x => x.SessionId == sessionId);

                    if (existingSession != null)
                    {
                        // Update existing session
                        existingSession.PageUrl = url;
                        existingSession.Username = userName;
                        existingSession.IpAddress = ip;
                        existingSession.DateModified = DateTime.Now;
                        _visitorSessionService.Update(existingSession);
                    }
                    else
                    {
                        // Create new session
                        var newSession = new VisitorSession
                        {
                            SessionId = sessionId,
                            IpAddress = ip,
                            PageUrl = url,
                            Username = userName,
                            DateCreated = DateTime.Now,
                            DateModified = DateTime.Now
                        };
                        _visitorSessionService.Add(newSession);
                    }
                }
                catch
                {
                    // Silently fail to not break page rendering
                }
            }
        }

        private static string? GetAcceptedLanguage(HttpRequest req)
        {
            var accept = req.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(accept)) return null;
            var first = accept.Split(',').FirstOrDefault();
            return string.IsNullOrWhiteSpace(first) ? null : first;
        }

        private static (string? id, string? name, bool isAuth) GetUser(HttpContext http)
        {
            var user = http.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
                var name = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Name);
                return (id, name, true);
            }
            return (null, null, false);
        }

        private static string? GetClientIp(HttpContext http)
        {
            if (http.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
            {
                var first = xff.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first)) return first;
            }
            return http.Connection.RemoteIpAddress?.ToString();
        }
    }
}
