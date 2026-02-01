using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public record CreateOrderCommand(
    Guid CustomerId,
    Guid EventId,
    string EventName,
    string CustomerEmail,
    string CustomerName,
    DateTime eventStartDate,
    string VenueName,
    string VenueCity,
    List<OrderItemDto> Items
) : IRequest<Result<Guid>>;
}
