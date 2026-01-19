using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.SharedKernel.ApiResponses;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Test successful response
        /// </summary>
        [HttpGet("success")]
        public IActionResult TestSuccess()
        {
            var data = new { message = "This is a successful response", timestamp = DateTime.UtcNow };
            return Ok(ApiResponse<object>.SuccessResponse(data, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Test validation error
        /// </summary>
        [HttpGet("validation-error")]
        public IActionResult TestValidationError()
        {
            var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email must be valid" } },
            { "Password", new[] { "Password must be at least 8 characters" } }
        };

            throw new ValidationException(errors);
        }

        /// <summary>
        /// Test not found error
        /// </summary>
        [HttpGet("not-found")]
        public IActionResult TestNotFound()
        {
            throw new NotFoundException("User", Guid.NewGuid());
        }

        /// <summary>
        /// Test unauthorized error
        /// </summary>
        [HttpGet("unauthorized")]
        public IActionResult TestUnauthorized()
        {
            throw new UnauthorizedException("You must be logged in to access this resource");
        }

        /// <summary>
        /// Test forbidden error
        /// </summary>
        [HttpGet("forbidden")]
        public IActionResult TestForbidden()
        {
            throw new ForbiddenException("You don't have permission to access this resource");
        }

        /// <summary>
        /// Test conflict error
        /// </summary>
        [HttpGet("conflict")]
        public IActionResult TestConflict()
        {
            throw new ConflictException("A user with this email already exists");
        }

        /// <summary>
        /// Test internal server error
        /// </summary>
        [HttpGet("server-error")]
        public IActionResult TestServerError()
        {
            throw new Exception("An unexpected error occured");
        }

        /// <summary>
        /// Test logging
        /// </summary>
        [HttpGet("logging")]
        public IActionResult TestLogging()
        {
            _logger.LogDebug("This is a debug message");
            _logger.LogInformation("This is an information message");
            _logger.LogWarning("This is a warning message");
            _logger.LogError("This is an error message");

            return Ok(ApiResponse<object>.SuccessResponse(
                new { message = "Check the logs folder for log entries" },
                HttpContext.TraceIdentifier));
        }
    }
}
