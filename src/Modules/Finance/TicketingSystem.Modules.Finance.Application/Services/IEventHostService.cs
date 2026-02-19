using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public interface IEventHostService
    {
        /// <summary>
        /// Resolves the HostId for a given EventId.
        /// Used by Finance handlers to determine which LIA-HOST-{id} account to debit.
        /// </summary>
        Task<Result<Guid>> GetHostIdAsync(Guid eventId, CancellationToken ct = default);
    }
}
