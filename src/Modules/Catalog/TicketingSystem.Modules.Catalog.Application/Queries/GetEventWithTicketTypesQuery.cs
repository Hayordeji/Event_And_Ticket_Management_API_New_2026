using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    public record GetEventWithTicketTypesQuery(Guid EventId) : IRequest<EventDetailResponse>;

}
