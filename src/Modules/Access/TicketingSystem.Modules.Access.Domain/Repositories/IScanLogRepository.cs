using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Domain.Entities;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Access.Domain.Repositories
{
    public interface IScanLogRepository : IRepository<ScanLog>
    {
        Task<List<ScanLog>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<List<ScanLog>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
        Task<bool> HasBeenScannedSuccessfullyAsync(Guid ticketId, CancellationToken cancellationToken = default);
        Task<int> GetAllowedCountAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<int> GetDeniedCountAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
