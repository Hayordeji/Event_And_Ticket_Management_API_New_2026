using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Domain.Events;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Entitites
{
    public class TicketDelivery : AggregateRoot
    {
        public Guid OrderId { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public Guid CustomerId { get; private set; }
        public string RecipientEmail { get; private set; } = string.Empty;

        // Delivery Information
        public DeliveryStatus Status { get; private set; }
        public DeliveryMethod Method { get; private set; }
        public int AttemptCount { get; private set; }
        public DateTime? SentAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public string? FailureReason { get; private set; }

        // Email Provider Information
        public string? EmailProvider { get; private set; }
        public string? EmailMessageId { get; private set; }
        public string? EmailResponse { get; private set; }

        // Ticket References (for batch delivery)
        public List<Guid> TicketIds { get; private set; } = new();

        // EF Core Constructor
        private TicketDelivery() { }

        public static TicketDelivery Create(
            Guid orderId,
            string orderNumber,
            Guid customerId,
            string recipientEmail,
            List<Guid> ticketIds,
            DeliveryMethod method = DeliveryMethod.Email)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                throw new ArgumentException("Recipient email is required", nameof(recipientEmail));

            if (!ticketIds.Any())
                throw new ArgumentException("At least one ticket ID is required", nameof(ticketIds));

            return new TicketDelivery
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                OrderNumber = orderNumber,
                CustomerId = customerId,
                RecipientEmail = recipientEmail,
                TicketIds = ticketIds,
                Method = method,
                Status = DeliveryStatus.Pending,
                AttemptCount = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsSending(string emailProvider)
        {
            Status = DeliveryStatus.Sending;
            EmailProvider = emailProvider;
            AttemptCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsSent(string emailMessageId, string emailResponse)
        {
            Status = DeliveryStatus.Sent;
            SentAt = DateTime.UtcNow;
            EmailMessageId = emailMessageId;
            EmailResponse = emailResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason)
        {
            Status = DeliveryStatus.Failed;
            FailedAt = DateTime.UtcNow;
            FailureReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDelivered(DateTime? deliveredAt = null)
        {
            Status = DeliveryStatus.Delivered;
            DeliveredAt = deliveredAt ?? DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            // ADD THESE LINES:
            RaiseDomainEvent(new TicketDeliveredEvent(
                Id,
                OrderId,
                OrderNumber,
                CustomerId,
                RecipientEmail,
                TicketIds));
        }
        public bool CanRetry()
        {
            // Allow up to 3 retry attempts
            return Status == DeliveryStatus.Failed && AttemptCount < 3;
        }

        public void PrepareForRetry()
        {
            if (!CanRetry())
                throw new InvalidOperationException("Delivery cannot be retried");

            Status = DeliveryStatus.Pending;
            FailedAt = null;
            FailureReason = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
