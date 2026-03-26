using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(UserManager<User> userManager, IEmailService emailService, ILogger<ForgotPasswordCommandHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Always return success (don't reveal if email exists)
            if (user == null) return Result.Success();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = $"https://placeholder.com/reset-password?email={user.Email}&token={encodedToken}";

            var emailResult = await _emailService.SendPasswordResetAsync(
                user.Email!, user.UserName!, resetLink, ct);

            if (!emailResult.IsSuccess)
                _logger.LogWarning("Password reset email failed for {Email}", user.Email);

            return Result.Success();
        }
    }
}
