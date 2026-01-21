using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Application.DTOs;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Commands
{
    /// <summary>
/// Command to create a new ledger account
/// </summary>
public record CreateLedgerAccountCommand(
    string AccountName,
    string AccountCode,
    AccountType AccountType,
    string Currency = "NGN",
    string? Description = null) : IRequest<Result<LedgerAccountResponse>>;
}
