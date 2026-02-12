using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Events
{
    /// <summary>
    /// Raised when tickets are successfully delivered to customer
    /// </summary>
    public sealed record TicketDeliveredEvent(
        Guid DeliveryId,
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string RecipientEmail,
        List<Guid> TicketIds) : DomainEvent;
}
