using System.Diagnostics;

namespace TicketingSystem.Api.Middleware
{
    /// <summary>
/// Logs all incoming requests and outgoing responses
/// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var traceId = context.TraceIdentifier;

            try
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} started. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    traceId);

                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    traceId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed in {ElapsedMs}ms. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    traceId);

                throw; // Re-throw to be handled by ExceptionHandlingMiddleware
            }
        }
    }
}
