using System.Net;
using System.Text.Json;
using TicketingSystem.SharedKernel.ApiResponses;
using TicketingSystem.SharedKernel.Exceptions;


namespace TicketingSystem.Api.Middleware
{
    ///< summary >
    /// Global exception handling middleware
    /// Catches all unhandled exceptions and returns consistent error responses
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;

            _logger.LogError(exception,
                "An error occurred. TraceId: {TraceId}. Path: {Path}",
                traceId,
                context.Request.Path);

            var (statusCode, error, errorDetails) = exception switch
            {
                NotFoundException notFound =>
                    (HttpStatusCode.NotFound, notFound.Message, null as object),

                ValidationException validation =>
                    (HttpStatusCode.BadRequest, "Validation failed.", validation.Errors),

                UnauthorizedException unauthorized =>
                    (HttpStatusCode.Unauthorized, unauthorized.Message, null as object),

                ForbiddenException forbidden =>
                    (HttpStatusCode.Forbidden, forbidden.Message, null as object),

                ConflictException conflict =>
                    (HttpStatusCode.Conflict, conflict.Message, null as object),

                DomainException domain =>
                    (HttpStatusCode.BadRequest, domain.Message, null as object),

                _ =>
                    (HttpStatusCode.InternalServerError,
                     "An internal server error occurred.",
                     _environment.IsDevelopment() ? exception.ToString() : null as object)
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse.ErrorResponse(error, errorDetails, traceId);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
