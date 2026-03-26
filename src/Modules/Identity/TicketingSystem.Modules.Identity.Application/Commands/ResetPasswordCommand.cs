using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword) : IRequest<Result>;
}
