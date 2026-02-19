using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Events
{
    public record OrderRefundedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal RefundAmount,
    decimal ServiceFee,
    string Currency,
    string PaymentReference,  
    string RefundReason,
    string PaymentGateway,
    List<ExpiredOrderItem> Items,  
    DateTime RefundedAt
    ) : DomainEvent;

   
}
