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
    Guid HostEventId,            
    decimal GrandTotal,     
    decimal ServiceFee,    
    string Currency,        
    string CancellationReason,
    DateTime CancelledAt
    ) : DomainEvent;
}
