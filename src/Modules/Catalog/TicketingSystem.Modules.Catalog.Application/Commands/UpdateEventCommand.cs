using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public record UpdateEventCommand(
    Guid EventId,
    Guid HostId,
    UpdateEventRequest Request) : IRequest<Result>;
}
