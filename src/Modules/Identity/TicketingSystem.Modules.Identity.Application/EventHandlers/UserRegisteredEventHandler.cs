using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Domain.Events;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Identity.Application.EventHandlers
{
    /// <summary>
    /// Handles UserRegisteredEvent by sending email verification link to new users
    /// Allows users to confirm their email address before accessing the system
    /// </summary>
    public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(
            UserManager<User> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<UserRegisteredEventHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Handle(
            UserRegisteredEvent notification,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing user registration email verification for UserId={UserId}, Email={Email}",
                    notification.UserId,
                    notification.Email);

                // Idempotency guard: fetch user ? if not found or already confirmed, skip
                var user = await _userManager.FindByIdAsync(notification.UserId.ToString());

                if (user == null)
                {
                    _logger.LogWarning(
                        "User not found for UserRegisteredEvent. UserId={UserId}, Email={Email}",
                        notification.UserId,
                        notification.Email);
                    return;
                }

                // If email already confirmed, skip sending verification
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation(
                        "Email already confirmed for user {Email}. Skipping verification email.",
                        notification.Email);
                    return;
                }

                _logger.LogInformation(
                    "Generating email confirmation token for user {Email}",
                    notification.Email);

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Encode token for safe transmission in URL
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Build verification link
                var baseUrl = _configuration["App:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    _logger.LogError(
                        "App:BaseUrl configuration is missing. Cannot generate verification link for user {Email}",
                        notification.Email);
                    return;
                }

                var verificationLink = $"{baseUrl}/api/auth/confirm-email?email={Uri.EscapeDataString(notification.Email)}&token={encodedToken}";

                _logger.LogDebug(
                    "Sending email verification to {Email}. VerificationLink={Link}",
                    notification.Email,
                    verificationLink);

                // Send verification email
                var emailResult = await _emailService.SendEmailVerificationAsync(
                    recipientEmail: notification.Email,
                    recipientName: notification.Name,
                    verificationLink: verificationLink,
                    ct: cancellationToken);

                if (!emailResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to send email verification for user {Email}. Response: {Response}",
                        notification.Email,
                        emailResult.Response);
                }
                else
                {
                    _logger.LogInformation(
                        "Email verification sent successfully to {Email}. MessageId={MessageId}",
                        notification.Email,
                        emailResult.MessageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing UserRegisteredEvent for UserId={UserId}, Email={Email}",
                    notification.UserId,
                    notification.Email);
                // Don't throw — let Outbox handle the retry
            }
        }
    }
}