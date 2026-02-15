using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Events
{
    /// <summary>
    /// Raised when a ticket is cancelled (refund scenario)
    /// </summary>
    public sealed record TicketCancelledEvent(
        Guid TicketId,
        string TicketNumber,
        string Reason) : DomainEvent;
}
