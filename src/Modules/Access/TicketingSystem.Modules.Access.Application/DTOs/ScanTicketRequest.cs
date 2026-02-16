using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Application.DTOs
{
    public record ScanTicketRequest(
    string QrCodeData,
    string DeviceId,
    string GateLocation);
}
