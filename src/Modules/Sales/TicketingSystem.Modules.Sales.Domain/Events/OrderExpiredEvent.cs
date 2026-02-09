using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderExpiredEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    DateTime ExpiredAt
) : DomainEvent;
}
