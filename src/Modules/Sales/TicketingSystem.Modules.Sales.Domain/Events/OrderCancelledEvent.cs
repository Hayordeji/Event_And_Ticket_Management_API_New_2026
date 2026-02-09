using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderCancelledEvent(
     Guid OrderId,
     string OrderNumber,
     Guid CustomerId,
     string CancellationReason,
     DateTime CancelledAt
 ) : DomainEvent;
}
