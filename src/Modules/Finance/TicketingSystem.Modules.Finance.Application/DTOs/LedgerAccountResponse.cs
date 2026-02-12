using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;

namespace TicketingSystem.Modules.Finance.Application.DTOs
{
    public record LedgerAccountResponse(
    Guid Id,
    string AccountName,
    string AccountCode,
    AccountType AccountType,
    decimal CurrentBalance,
    string Currency,
    string Description,
    bool IsActive,
    DateTime CreatedAt
    );
}
