using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Events
{
    /// <summary>
    /// Raised when a ticket is scanned at event entrance
    /// Used for attendance tracking and analytics
    /// </summary>
    public sealed record TicketScannedEvent(
        Guid TicketId,
        string TicketNumber,
        Guid CustomerId,
        Guid EventId,
        Guid ScannedBy,
        string ScanLocation) : DomainEvent;
}
