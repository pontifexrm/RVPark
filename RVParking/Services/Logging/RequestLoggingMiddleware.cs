using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RVParking.Data;

namespace RVParking.Services.Logging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Only log requests that look like navigations to routed pages.
            if (IsNavigationRequest(context))
            {
                // Resolve DbContext from DI
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var userId = context.User?.Identity?.IsAuthenticated == true
                    ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    : null;

                var visit = new VisitLog
                {
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    UserId = userId,
                    Timestamp = DateTimeOffset.UtcNow
                };

                db.VisitLogs.Add(visit);
                await db.SaveChangesAsync();
            }

            await _next(context);
        }

        private static bool IsNavigationRequest(HttpContext context)
        {
            var req = context.Request;

            // Only consider plain GET navigations
            if (!HttpMethods.IsGet(req.Method))
                return false;

            // Best-effort check: browser navigations will typically accept HTML.
            var accept = req.Headers["Accept"].ToString();
            if (!accept.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                return false;

            var path = req.Path.Value ?? string.Empty;

            // Ignore common static/framework paths
            if (path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("//wp", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("//xmlrpc.php", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/wp-admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, "/favicon.ico", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, "/robots.txt", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // If the last segment contains a dot, it's probably a file (logo.png, app.js, etc.)
            var lastSegment = Path.GetFileName(path);
            if (lastSegment.Contains("."))
                return false;

            // At this point it's likely a routed navigation like "/allbookings"
            return true;
        }
    }
}
