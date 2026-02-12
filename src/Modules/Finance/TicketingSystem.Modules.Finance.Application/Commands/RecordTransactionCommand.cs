using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Commands
{
    /// <summary>
    /// Command to record a financial transaction
    /// </summary>
    public record RecordTransactionCommand(
    string ReferenceType,
    Guid ReferenceId,
    string Description,
    DateTime OccurredAt,
    List<TransactionEntryRequest> Entries) : IRequest<Result<TransactionResponse>>;
}
