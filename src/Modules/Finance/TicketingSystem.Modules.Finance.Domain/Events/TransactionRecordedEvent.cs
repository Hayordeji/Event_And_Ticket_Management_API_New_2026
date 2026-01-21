using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.Events
{
    ///<summary>
/// Raised when a financial transaction is recorded
/// </summary>
    public record TransactionRecordedEvent
    (Guid TransactionId,
    string ReferenceType,
    Guid ReferenceId,
    decimal TotalAmount,
    string Currency,
    DateTime OccurredAt) : DomainEvent;
}
