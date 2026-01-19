namespace TicketingSystem.Api.Middleware
{
    /// <summary>
/// Adds security headers to all responses
/// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Remove server header (security through obscurity)
            context.Response.Headers.Remove("Server");

            // Prevent clickjacking
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // Prevent MIME type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Enable browser XSS protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

            // Referrer policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy (adjust based on your needs)
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

            // Permissions Policy (formerly Feature-Policy)
            context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

            await _next(context);
        }
    }
}
