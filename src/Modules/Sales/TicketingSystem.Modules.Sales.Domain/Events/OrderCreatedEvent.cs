using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderCreatedEvent(
    Guid OrderId,
    Guid MainEventId,
    Guid UserId,
    string OrderNumber,
    //decimal TotalAmount,
    //string Currency,
    DateTime CreatedAt
    ) : DomainEvent;
}
