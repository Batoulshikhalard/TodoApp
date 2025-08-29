// Middleware/EnhancedRateLimitMiddleware.cs
using Microsoft.Extensions.Caching.Memory;

namespace TodoApp.Web.Middleware
{
    public class EnhancedRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EnhancedRateLimitMiddleware> _logger;

        public EnhancedRateLimitMiddleware(RequestDelegate next, IMemoryCache cache,
            ILogger<EnhancedRateLimitMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
            {
                await _next(context);
                return;
            }

            var requestPath = context.Request.Path;
            var cacheKey = $"{ipAddress}_{requestPath}";

            // Check if IP is blocked
            var isBlocked = _cache.Get<bool>($"{ipAddress}_blocked");
            if (isBlocked)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("IP address temporarily blocked due to excessive requests.");
                _logger.LogWarning($"Blocked request from {ipAddress} to {requestPath}");
                return;
            }

            // Get request history
            var requestHistory = _cache.GetOrCreate<List<DateTime>>(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return new List<DateTime>();
            });

            // Remove requests older than 1 minute
            requestHistory.RemoveAll(t => DateTime.UtcNow - t > TimeSpan.FromMinutes(1));

            // Check if rate limit exceeded
            if (requestHistory.Count >= 60) // 60 requests per minute
            {
                // Block IP for 5 minutes
                _cache.Set($"{ipAddress}_blocked", true, TimeSpan.FromMinutes(5));

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too many requests. IP address blocked for 5 minutes.");
                _logger.LogWarning($"Rate limit exceeded by {ipAddress} to {requestPath}. IP blocked.");
                return;
            }

            // Add current request to history
            requestHistory.Add(DateTime.UtcNow);
            _cache.Set(cacheKey, requestHistory, TimeSpan.FromMinutes(5));

            await _next(context);
        }
    }

    // Extension method
    public static class EnhancedRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseEnhancedRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnhancedRateLimitMiddleware>();
        }
    }
}