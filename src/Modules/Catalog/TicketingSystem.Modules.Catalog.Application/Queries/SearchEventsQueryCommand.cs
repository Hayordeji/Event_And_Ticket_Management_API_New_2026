using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    /// <summary>
    /// Query to search events with filters and pagination
    /// </summary>
    public record SearchEventsQueryCommand(SearchEventsRequest Request) 
        : IRequest<Result<(List<EventResponse> Events, int TotalCount)>>;
}
