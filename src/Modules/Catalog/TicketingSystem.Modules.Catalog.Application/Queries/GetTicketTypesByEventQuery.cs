using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    
     public record GetTicketTypesByEventQuery(Guid EventId) : IRequest<Result<List<TicketTypeResponse>>>;

   
}
