using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Application.DTOs
{
    public record EventScanStatsResponse(
    Guid EventId,
    int TotalAllowed,
    int TotalDenied,
    int TotalScans);

}
