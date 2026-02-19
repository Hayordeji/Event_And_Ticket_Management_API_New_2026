using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services
{
    /// <summary>
    /// Handles refund operations with payment gateways.
    /// Separate from IPaymentGatewayService to follow Interface Segregation Principle.
    /// </summary>
    public interface IPaymentGatewayRefundService
    {
        /// <summary>
        /// Initiates a refund for a previously successful payment.
        /// </summary>
        Task<Result<RefundResponse>> InitiateRefundAsync(
            string paymentReference,
            decimal amount,
            string currency,
            string reason,
            CancellationToken ct = default);

        /// <summary>
        /// Gateway name for logging/debugging.
        /// </summary>
        string GatewayName { get; }
    }

    public record RefundResponse(
    string RefundReference,
    string Status,
    string Message,
    DateTime ProcessedAt
);
}
