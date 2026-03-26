using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
using TicketingSystem.SharedKernel.Exceptions;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginResponse>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenGenerator _jwtTokenService;
        private readonly IdentityAppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<LoginUserCommandHandler> _logger;
        public LoginUserCommandHandler(
            UserManager<User> userManager,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenGenerator jwtTokenService,
            IdentityAppDbContext context,
            IEmailService emailService,
            ILogger<LoginUserCommandHandler> logger,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<Result<LoginResponse>> Handle(
            LoginUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.IsActive)
                return Result.Failure<LoginResponse>("Invalid email or password.");



            if (await _userManager.IsLockedOutAsync(user))
            {
                var locksAt = user.LockoutEnd?.UtcDateTime ?? DateTime.UtcNow.AddMinutes(15);
                await _emailService.SendAccountLockedAsync(user.Email!, user.UserName!, locksAt, cancellationToken);
                return Result.Failure<LoginResponse>("Account locked. Try again in 15 minutes.");
            }


            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                return Result.Failure<LoginResponse>("Invalid email or password.");


            // Get role for JWT claim
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            // Generate JWT
            var accessToken = _jwtTokenService.GenerateAccessToken(user, role);

            // Generate and persist refresh token
            var refreshToken = RefreshToken.Create(
                userId: user.Id,
                deviceInfo: request.DeviceInfo);

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new LoginResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                RefreshTokenExpiresAt: refreshToken.ExpiresAt,
                UserId: user.Id,
                Email: user.Email!,
                FullName: user.FullName,
                Role: role));
        }
    }
}
