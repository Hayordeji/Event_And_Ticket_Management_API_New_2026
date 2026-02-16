using MediatR;
using Microsoft.AspNetCore.Identity;
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
using TicketingSystem.SharedKernel.Authorization;
using TicketingSystem.SharedKernel.Exceptions;
using TicketingSystem.SharedKernel.Persistence;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly UserManager<User> _userManager;

        public RegisterUserCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<Guid>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
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
                UserName = request.Email.ToLowerInvariant().Trim(), // Identity requires UserName
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                IsActive = true,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            // UserManager handles hashing — PBKDF2 by default
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return Result.Failure<Guid>(
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
                return Result.Failure<Guid>(
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));

            return Result.Success(user.Id);
        }
    }
}
