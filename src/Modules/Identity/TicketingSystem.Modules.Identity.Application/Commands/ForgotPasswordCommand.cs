using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Application.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result>;

}
