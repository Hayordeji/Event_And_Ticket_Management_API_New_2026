using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record CreateOrderRequest(
    Guid EventId,
    Guid CustomerId,
    string EventName,
    string CustomerEmail,
    string CustomerName,
    DateTime eventStartDate,
    string VenueName,
    string VenueCity,
    List<OrderItemDto> Items
    );

    public record OrderItemDto(
        Guid TicketTypeId,
        string TicketTypeName,
        string TicketTypeDescription,
        int Quantity,
        decimal UnitPrice
    );
}
