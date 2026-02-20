using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Domain.Events
{
    public record OrderPaidEvent(
     Guid OrderId,
     Guid HostEventId,
     string OrderNumber,
     decimal TotalAmount,
     string Currency,
     DateTime PaidAt,
     Guid CustomerId,
     //Guid HostId,
     string PaymentReference
    ) : DomainEvent;
}
