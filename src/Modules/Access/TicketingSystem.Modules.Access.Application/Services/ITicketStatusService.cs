using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Access.Application.Services
{
    public interface ITicketStatusService
    {
        Task MarkAsUsedAsync(
        Guid ticketId,
        Guid scannedBy,
        string gateLocation,
        CancellationToken cancellationToken = default);
    }
}
