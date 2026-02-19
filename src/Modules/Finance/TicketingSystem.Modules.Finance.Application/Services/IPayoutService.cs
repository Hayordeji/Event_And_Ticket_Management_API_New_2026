using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services
{
    public interface IPayoutService
    {
        /// <summary>
        /// Initiates a bank transfer to a host's account.
        /// </summary>
        Task<Result<PayoutResponse>> InitiatePayoutAsync(
            Guid hostId,
            decimal amount,
            string currency,
            string bankAccountNumber,
            string bankCode,
            string reason,
            CancellationToken ct = default);
    }
    public record PayoutResponse(
    string PayoutReference,
    string Status,
    string Message,
    DateTime ProcessedAt
);
}
