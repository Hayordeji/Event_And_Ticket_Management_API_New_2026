using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;

namespace TicketingSystem.Modules.Finance.Application.DTOs
{
    public record RecordTransactionRequest(
    string ReferenceType,
    Guid ReferenceId,
    string Description,
    List<TransactionEntryRequest> Entries);

    public record TransactionEntryRequest(
        string AccountCode,
        decimal Amount,
        string Currency,
        EntryType EntryType,
        string? Description = null);
}
