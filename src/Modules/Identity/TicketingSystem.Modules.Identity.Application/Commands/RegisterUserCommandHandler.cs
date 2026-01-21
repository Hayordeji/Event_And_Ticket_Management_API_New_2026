using MediatR;
using Microsoft.EntityFrameworkCore.Internal;
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
using TicketingSystem.SharedKernel.Exceptions;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IdentityDbContext _context;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IdentityDbContext context)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _context = context;
        }

        public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Validate password confirmation
            if (request.Password != request.ConfirmPassword)
                return Result.Failure<AuthResponse>("Passwords do not match");

            // Check email uniqueness
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
                return Result.Failure<AuthResponse>(emailResult.Error);

            var emailExists = await _userRepository.EmailExistsAsync(emailResult.Value, cancellationToken);
            if (emailExists)
                return Result.Failure<AuthResponse>("A user with this email already exists");

            // Create user
            var userResult = User.Create(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber);

            if (userResult.IsFailure)
                return Result.Failure<AuthResponse>(userResult.Error);

            var user = userResult.Value;
          
            // Save user
            await _userRepository.AddAsync(user, cancellationToken);

            // Generate tokens
            var deviceFingerprint = DeviceFingerprint.Create(request.UserAgent, request.IpAddress);
            var (accessToken, refreshToken, expiresAt) = _jwtTokenGenerator.GenerateTokens(user);

            // Add refresh token to user
            user.AddRefreshToken(refreshToken, expiresAt.AddDays(7), deviceFingerprint);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new AuthResponse(
                user.Id,
                user.Email.Value,
                user.FirstName,
                user.LastName,
                accessToken,
                refreshToken,
                expiresAt);

            return Result.Success(response);
        }
    }
}
