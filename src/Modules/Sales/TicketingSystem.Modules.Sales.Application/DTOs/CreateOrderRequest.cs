using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record CreateOrderRequest(
    Guid EventId,
    Guid CustomerId,
    string CustomerEmail,
    string CustomerName,
    List<OrderItemDto> Items
    );

    public record OrderItemDto(
        Guid TicketTypeId,
        int Quantity,
        decimal UnitPrice
    );
}
