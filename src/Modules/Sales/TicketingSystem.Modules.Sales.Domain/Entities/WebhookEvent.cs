using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.Entities
{
    public class WebhookEvent : Entity
    {
        public string GatewayEventId { get; private set; } = null!;
        public string Gateway { get; private set; } = null!;
        public string EventType { get; private set; } = null!;
        public string PaymentReference { get; private set; } = null!;
        public bool IsProcessed { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string RawPayload { get; private set; } = null!;

        private WebhookEvent() { } // EF Core

        public static WebhookEvent Create(
            string gatewayEventId,
            string gateway,
            string eventType,
            string paymentReference,
            string rawPayload)
        {
            if (string.IsNullOrWhiteSpace(gatewayEventId))
                throw new ArgumentNullException(nameof(gatewayEventId));

            if (string.IsNullOrWhiteSpace(gateway))
                throw new ArgumentNullException(nameof(gateway));

            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentNullException(nameof(eventType));

            if (string.IsNullOrWhiteSpace(paymentReference))
                throw new ArgumentNullException(nameof(paymentReference));

            return new WebhookEvent
            {
                Id = Guid.NewGuid(),
                GatewayEventId = gatewayEventId,
                Gateway = gateway,
                EventType = eventType,
                PaymentReference = paymentReference,
                RawPayload = rawPayload ?? string.Empty,
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
            ProcessedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
