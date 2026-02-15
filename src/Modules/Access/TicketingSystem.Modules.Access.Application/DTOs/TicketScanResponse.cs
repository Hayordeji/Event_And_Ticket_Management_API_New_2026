using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Application.DTOs
{
    public record ScanTicketResponse(
    bool IsAllowed,
    string TicketNumber,
    string? TicketTypeName,
    string? CustomerName,
    string? DenialReason,
    string? DenialMessage,
    DateTime ScannedAt);
}
