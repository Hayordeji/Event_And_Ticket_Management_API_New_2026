using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Domain.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    /// <summary>
    /// Query to search Host's events with filters and pagination
    /// </summary>
    public record SearchHostEventsQueryCommand(SearchHostEventsRequest Request, Guid HostId) 
        : IRequest<Result<(List<EventResponse> Events, int TotalCount)>>;
}
