using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Outbox
{
    /// <summary>
    /// Represents a domain event that has been persisted to the outbox
    /// for reliable, at-least-once delivery with retry and dead-letter support.
    /// </summary>
    public class OutboxMessage
    {
        public Guid Id { get; private set; }

        /// <summary>
        /// Full CLR type name of the event (e.g., "TicketingSystem.Modules.Sales.Domain.Events.OrderPaidEvent")
        /// Used to deserialize the JSON payload back into the concrete event type.
        /// </summary>
        public string EventType { get; private set; } = string.Empty;

        /// <summary>
        /// JSON-serialized event payload.
        /// </summary>
        public string EventPayload { get; private set; } = string.Empty;

        /// <summary>
        /// When the event occurred in the domain (NOT when it was written to the outbox).
        /// </summary>
        public DateTime OccurredAt { get; private set; }

        /// <summary>
        /// When the outbox message was persisted to the database.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// When this message was successfully processed and marked as completed.
        /// </summary>
        public DateTime? ProcessedAt { get; private set; }

        /// <summary>
        /// Current processing status.
        /// </summary>
        public OutboxMessageStatus Status { get; private set; }

        /// <summary>
        /// Number of times processing has been attempted.
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// When the next retry should be attempted (null if not scheduled).
        /// </summary>
        public DateTime? RetryAt { get; private set; }

        /// <summary>
        /// Last error message if processing failed.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        private OutboxMessage() { } // EF Core

        public static OutboxMessage Create(
            string eventType,
            string eventPayload,
            DateTime occurredAt)
        {
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                EventPayload = eventPayload,
                OccurredAt = occurredAt,
                CreatedAt = DateTime.UtcNow,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0
            };
        }

        /// <summary>
        /// Mark this message as successfully processed.
        /// </summary>
        public void MarkAsProcessed()
        {
            Status = OutboxMessageStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
            ErrorMessage = null;
            RetryAt = null;
        }

        /// <summary>
        /// Record a failed processing attempt and schedule a retry with exponential backoff.
        /// After maxRetries, the message is moved to DeadLettered status.
        /// </summary>
        public void RecordFailure(string errorMessage, int maxRetries = 5)
        {
            RetryCount++;
            ErrorMessage = errorMessage;

            if (RetryCount >= maxRetries)
            {
                Status = OutboxMessageStatus.DeadLettered;
                RetryAt = null;
            }
            else
            {
                Status = OutboxMessageStatus.Pending;
                // Exponential backoff: 30s, 1m, 5m, 15m, 30m
                var delayMinutes = RetryCount switch
                {
                    1 => 0.5,  // 30 seconds
                    2 => 1,
                    3 => 5,
                    4 => 15,
                    _ => 30
                };
                RetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
            }
        }
    }
}
