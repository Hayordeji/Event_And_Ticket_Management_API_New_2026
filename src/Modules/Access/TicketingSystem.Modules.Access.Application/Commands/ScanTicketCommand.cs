using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Application.Commands
{
    public record ScanTicketCommand(
    string QrCodeData,
    Guid ScannedBy,
    string DeviceId,
    string GateLocation) : IRequest<Result<ScanTicketResponse>>;
}
