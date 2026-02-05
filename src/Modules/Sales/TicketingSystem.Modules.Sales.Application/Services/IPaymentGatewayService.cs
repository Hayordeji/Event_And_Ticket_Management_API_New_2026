using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services
{
    public interface IPaymentGatewayService
    {
        /// <summary>
        /// Initialize a payment session with the gateway
        /// </summary>
        Task<Result<PaymentInitializationResponse>> InitializePaymentAsync(
            string orderNumber,
            decimal amount,
            string currency,
            string customerEmail,
            string callbackUrl,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify a payment with the gateway
        /// </summary>
        Task<Result<PaymentVerificationResponse>> VerifyPaymentAsync(
            string paymentReference,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify webhook signature
        /// </summary>
        bool VerifyWebhookSignature(string signature, string payload);

        /// <summary>
        /// Gateway name
        /// </summary>
        string GatewayName { get; }
    }
}
