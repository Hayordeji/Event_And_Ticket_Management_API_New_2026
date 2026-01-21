using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Application.DTOs;
using TicketingSystem.Modules.Identity.Application.Services;
using TicketingSystem.Modules.Identity.Domain.Repositories;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<AuthResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // Validate email format
            var emailResult = Email.Create(request.Email);
            if (emailResult.IsFailure)
                throw new UnauthorizedException("Invalid email or password");

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password");

            // Check if user is active
            if (!user.IsActive)
                throw new UnauthorizedException("Account is deactivated");

            // Verify password
            if (!user.VerifyPassword(request.Password))
                throw new UnauthorizedException("Invalid email or password");

            // Record login
            var deviceFingerprint = DeviceFingerprint.Create(request.UserAgent, request.IpAddress);
            user.RecordLogin(deviceFingerprint);

            // Generate tokens
            var (accessToken, refreshToken, expiresAt) = _jwtTokenGenerator.GenerateTokens(user);

            // Add refresh token
            user.AddRefreshToken(refreshToken, expiresAt.AddDays(7), deviceFingerprint);

            //await _unitOfWork.SaveChangesAsync(cancellationToken);

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
