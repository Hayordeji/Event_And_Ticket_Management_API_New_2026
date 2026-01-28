using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderRefundedEvent(
    Guid OrderId,
    string OrderNumber,
    decimal RefundAmount,
    string Currency,
    Guid CustomerId,
    string RefundReason,
    DateTime RefundedAt
) : DomainEvent;
}
