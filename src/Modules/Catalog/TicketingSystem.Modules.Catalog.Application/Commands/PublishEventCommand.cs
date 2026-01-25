using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
   public record PublishEventCommand(
    Guid EventId,
    Guid HostId) : IRequest<Result>;
}
