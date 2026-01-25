using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public record AddTicketTypeCommand(
    Guid EventId,
    Guid HostId,
    AddTicketTypeRequest Request) : IRequest<Result<Guid>>;
}
