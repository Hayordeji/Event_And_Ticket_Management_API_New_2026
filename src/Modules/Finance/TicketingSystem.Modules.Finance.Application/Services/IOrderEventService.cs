using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public interface IOrderEventService
    {
        /// <summary>
        /// Resolves the EventId for a given OrderId.
        /// Used by Finance refund handler since OrderRefundedEvent carries neither.
        /// </summary>
        Task<Result<Guid>> GetEventIdByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
