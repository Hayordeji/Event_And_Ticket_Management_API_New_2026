using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Queries
{
    public record GetCustomerOrdersQuery(
    Guid CustomerId
) : IRequest<Result<IEnumerable<OrderResponse>>>;
}
