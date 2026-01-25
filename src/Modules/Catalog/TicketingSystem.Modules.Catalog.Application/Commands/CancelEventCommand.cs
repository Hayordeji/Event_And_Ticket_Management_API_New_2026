using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public record CancelEventCommand(
    Guid EventId,
    Guid HostId,
    string Reason) : IRequest<Result>;
}
