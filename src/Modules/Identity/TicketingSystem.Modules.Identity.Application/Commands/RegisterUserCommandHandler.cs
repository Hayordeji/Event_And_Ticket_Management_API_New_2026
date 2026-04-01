using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Application.DTOs;
using TicketingSystem.Modules.Identity.Application.Services;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Domain.Repositories;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Authorization;
using TicketingSystem.SharedKernel.Exceptions;
using TicketingSystem.SharedKernel.Persistence;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(UserManager<User> userManager, ILogger<RegisterUserCommandHandler> logger, IEmailService emailService)
        {
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Result<Guid>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Guard: only Customer and Host can self-register
                if (request.Role is not (Roles.Customer or Roles.Host))
                    return Result.Failure<Guid>(
                        "Only Customer and Host roles can be assigned during registration.");

                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser is not null)
                    return Result.Failure<Guid>("A user with this email already exists.");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.ToLowerInvariant().Trim(),
                    UserName = $"{request.FirstName}{request.LastName}", // Identity requires UserName
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    IsActive = true,
                    EmailConfirmed = false,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };


                //Create User
                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                    return Result.Failure<Guid>(
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));



                //Assign Role
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!roleResult.Succeeded)
                    return Result.Failure<Guid>(
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));


                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var verificationLink = $"http://localhost:8080/api/auth/confirm-email?email={user.Email}&token={encodedToken}";

                var emailResult = await _emailService.SendEmailVerificationAsync(
                    user.Email!, user.UserName!, verificationLink, cancellationToken);

                if (!emailResult.IsSuccess)
                    _logger.LogWarning("Verification email failed for {Email}", user.Email);

                return Result.Success(user.Id);
            }
            catch (Exception ex)
            {
                return Result.Failure<Guid>(ex.Message);
                throw;
            }
        }
    }
}
