using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Application.Queries
{
    public sealed record GetCustomerTicketsQuery(Guid CustomerId, Guid requesterId) : IRequest<Result<List<TicketResponse>>>;

}
