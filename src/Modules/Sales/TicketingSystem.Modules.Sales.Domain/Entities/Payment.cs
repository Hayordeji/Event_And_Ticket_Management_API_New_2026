using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Sales.Domain.Enums;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Sales.Domain.Entities
{
    public class Payment : Entity
    {
        public Guid OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? PaymentReference { get; private set; } // Gateway reference (Paystack, Flutterwave)
        public string? GatewayResponse { get; private set; } // JSON response from gateway
        public DateTime? PaidAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public string? FailureReason { get; private set; }

        // Navigation property
        public Order Order { get; private set; } = null!;

        private Payment() { } // EF Core

        public static Payment Create(
            Guid orderId,
            decimal amount,
            string currency,
            PaymentMethod method,
            string paymentReference,
            string gatewayResponse)
        {
            if (amount <= 0)
                throw new DomainException("Payment amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException("Currency is required.");

            if (string.IsNullOrWhiteSpace(gatewayResponse))
                throw new DomainException("Gateway Response is required.");

            if (string.IsNullOrWhiteSpace(paymentReference))
                throw new DomainException("Payement Reference is required.");

            return new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                Currency = currency,
                Method = method,
                Status = PaymentStatus.Pending,
                PaymentReference = paymentReference,
                CreatedAt = DateTime.UtcNow,
                GatewayResponse = gatewayResponse
            };
        }

        public void SetPaymentReference(string paymentReference)
        {
            if (string.IsNullOrWhiteSpace(paymentReference))
                throw new ArgumentNullException(nameof(paymentReference));

            if (!string.IsNullOrEmpty(PaymentReference))
                throw new DomainException("Payment reference already set.");

            PaymentReference = paymentReference;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCompleted(string paymentReference, string gatewayResponse)
        {
            if (Status == PaymentStatus.Successful)
                throw new DomainException("Payment is already completed.");

            Status = PaymentStatus.Successful;
            PaymentReference = paymentReference;
            GatewayResponse = gatewayResponse;
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string failureReason, string gatewayResponse)
        {
            if (Status == PaymentStatus.Failed)
                throw new DomainException("Cannot mark completed payment as failed.");

            Status = PaymentStatus.Failed;
            FailureReason = failureReason;
            GatewayResponse = gatewayResponse;
            FailedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded()
        {
            if (Status != PaymentStatus.Refunded)
                throw new DomainException("Only completed payments can be refunded.");

            Status = PaymentStatus.Refunded;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
