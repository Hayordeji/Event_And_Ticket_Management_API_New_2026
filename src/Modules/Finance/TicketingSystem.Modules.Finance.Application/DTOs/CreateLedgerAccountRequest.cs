using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;

namespace TicketingSystem.Modules.Finance.Application.DTOs
{
    public record CreateLedgerAccountRequest(
    string AccountName,
    string AccountCode,
    AccountType AccountType,
    string Currency = "NGN",
    string? Description = null);
}
