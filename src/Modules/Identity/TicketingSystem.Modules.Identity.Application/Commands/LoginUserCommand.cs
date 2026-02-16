using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public record LoginUserCommand(
    string Email,
    string Password,
    string DeviceInfo
    ) : IRequest<Result<LoginResponse>>;

    public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    string Role
);
}
