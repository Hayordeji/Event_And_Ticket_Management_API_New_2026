using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Application.Services
{
    public interface ITicketStatusService
    {
        Task<Result> MarkAsUsedAsync(
        Guid ticketId,
        Guid scannedBy,
        string gateLocation,
        CancellationToken cancellationToken = default);
    }
}
