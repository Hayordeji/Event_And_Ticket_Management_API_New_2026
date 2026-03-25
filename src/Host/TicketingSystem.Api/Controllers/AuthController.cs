using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TicketingSystem.Modules.Identity.Application.Commands;
using TicketingSystem.Modules.Identity.Application.DTOs;
using TicketingSystem.SharedKernel.ApiResponses;

namespace TicketingSystem.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [EnableRateLimiting("fixed_auth_register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Modules.Identity.Application.DTOs.RegisterRequest request)
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Role);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(ApiResponse<Guid>.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Ok(ApiResponse<Guid>.SuccessResponse(result.Value, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [EnableRateLimiting("fixed_auth_login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Modules.Identity.Application.DTOs.LoginRequest request)
        {
            var command = new LoginUserCommand(
                request.Email,
                request.Password,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(result.Error, traceId: HttpContext.TraceIdentifier));

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(result.Value, HttpContext.TraceIdentifier));
        }

        /// <summary>
        /// Get current user info (requires authentication)
        /// </summary>
        [HttpGet("me")]
        [EnableRateLimiting("get_endpoints")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;

            var userInfo = new
            {
                UserId = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            return Ok(ApiResponse<object>.SuccessResponse(userInfo, HttpContext.TraceIdentifier));
        }

        [HttpGet("confirm-email")]
        [EnableRateLimiting("get_endpoints")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            var result = await _mediator.Send(new ConfirmEmailCommand(email, token));
            return result.IsSuccess ? Ok("Email confirmed.") : BadRequest(result.Error);
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("fixed_post_endpoints")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _mediator.Send(new ForgotPasswordCommand(request.Email));
            return Ok("If the email exists, a reset link has been sent."); // Always 200
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("fixed_post_endpoints")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _mediator.Send(
                new ResetPasswordCommand(request.Email, request.ResetCode, request.NewPassword));
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }



        /// <summary>
        /// Health check for auth module
        /// </summary>
        [HttpGet("health")]
        [EnableRateLimiting("get_endpoints")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { module = "Identity", status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
