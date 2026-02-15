using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Application.DTOs
{
    public record ScanLogResponse(
    Guid ScanLogId,
    string TicketNumber,
    string Result,
    string? DenialReason,
    string GateLocation,
    string DeviceId,
    DateTime ScannedAt);
}
