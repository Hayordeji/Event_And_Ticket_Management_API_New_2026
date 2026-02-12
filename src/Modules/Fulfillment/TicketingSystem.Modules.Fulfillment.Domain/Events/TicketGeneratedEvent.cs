using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Events
{
    /// <summary>
    /// Raised when a ticket is generated for a customer
    /// Triggers email delivery
    /// </summary>
    public sealed record TicketGeneratedEvent(
        Guid TicketId,
        string TicketNumber,
        Guid CustomerId,
        Guid EventId,
        string OrderNumber) : DomainEvent;
}
