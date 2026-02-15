using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Application.DTOs
{
    public record ScanTicketRequest(
    string QrCodeData,
    Guid EventId,
    string DeviceId,
    string GateLocation);
}
