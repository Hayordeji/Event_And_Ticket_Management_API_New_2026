using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderPaidEvent(
     Guid OrderId,
     string OrderNumber,
     decimal TotalAmount,
     string Currency,
     DateTime PaidAt,
     Guid CustomerId,
     string PaymentReference
 ) : DomainEvent;
}
