using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public record RegisterUserCommand(string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string UserAgent,
    string IpAddress) : IRequest<Result<AuthResponse>>;

}
