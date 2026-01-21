using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Finance.Application.DTOs
{
    public record TransactionResponse(
    Guid Id,
    string ReferenceType,
    Guid ReferenceId,
    string Description,
    DateTime OccurredAt,
    bool IsPosted,
    DateTime? PostedAt,
    List<EntryResponse> Entries);


    public record EntryResponse(
    Guid Id,
    string AccountName,
    string AccountCode,
    decimal Amount,
    string Currency,
    string EntryType,
    string Description);
}
