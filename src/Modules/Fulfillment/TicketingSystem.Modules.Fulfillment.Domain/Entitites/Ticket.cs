using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Domain.Events;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Domain.Entitites
{
    /// <summary>
    /// Ticket aggregate root representing a single ticket with QR code
    /// Contains immutable order snapshot data for historical accuracy
    /// </summary>
    public class Ticket : AggregateRoot
    {
        // Core Ticket Properties
        public string TicketNumber { get; private set; } = string.Empty;
        public Guid OrderId { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public Guid CustomerId { get; private set; }
        public Guid EventId { get; private set; }
        public Guid TicketTypeId { get; private set; }

        // Order Snapshot - Immutable event details at time of ticket generation
        public string EventName { get; private set; } = string.Empty;
        public DateTime EventStartDate { get; private set; }
        public DateTime EventEndDate { get; private set; }
        public string VenueName { get; private set; } = string.Empty;
        public string VenueAddress { get; private set; } = string.Empty;
        public string VenueCity { get; private set; } = string.Empty;
        public string TicketTypeName { get; private set; } = string.Empty;

        // Ticket Identification
        public string QrCodeData { get; private set; } = string.Empty;
        public string Barcode { get; private set; } = string.Empty;

        // Status Tracking
        public TicketStatus Status { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public Guid? ScannedBy { get; private set; }
        public string? ScanLocation { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        // Customer Information (for ticket display)
        public string CustomerEmail { get; private set; } = string.Empty;
        public string CustomerFirstName { get; private set; } = string.Empty;
        public string CustomerLastName { get; private set; } = string.Empty;

        // Pricing Information (snapshot from order)
        public decimal PricePaid { get; private set; }
        public string Currency { get; private set; } = "NGN";

        // EF Core Constructor
        private Ticket() { }

        /// <summary>
        /// Creates a new ticket with order snapshot data
        /// </summary>
        public static Ticket Create(
            Guid orderId,
            string orderNumber,
            Guid customerId,
            string customerEmail,
            string customerFirstName,
            string customerLastName,
            Guid eventId,
            string eventName,
            DateTime eventStartDate,
            DateTime eventEndDate,
            string venueName,
            string venueAddress,
            string venueCity,
            Guid ticketTypeId,
            string ticketTypeName,
            decimal pricePaid,
            string currency = "NGN")
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                TicketNumber = GenerateTicketNumber(),
                OrderId = orderId,
                OrderNumber = orderNumber,
                CustomerId = customerId,
                CustomerEmail = customerEmail,
                CustomerFirstName = customerFirstName,
                CustomerLastName = customerLastName,
                EventId = eventId,
                EventName = eventName,
                EventStartDate = eventStartDate,
                EventEndDate = eventEndDate,
                VenueName = venueName,
                VenueAddress = venueAddress,
                VenueCity = venueCity,
                TicketTypeId = ticketTypeId,
                TicketTypeName = ticketTypeName,
                PricePaid = pricePaid,
                Currency = currency,
                Status = TicketStatus.Valid,
                CreatedAt = DateTime.UtcNow
            };

            // Generate QR code data (encrypted ticket ID)
            ticket.QrCodeData = GenerateQrCodeData(ticket.Id, ticket.TicketNumber);
            ticket.Barcode = GenerateBarcode(ticket.Id);

            ticket.RaiseDomainEvent(new TicketGeneratedEvent(
                ticket.Id,
                ticket.TicketNumber,
                customerId,
                eventId,
                orderNumber));

            return ticket;
        }

        /// <summary>
        /// Marks ticket as used (scanned at entrance)
        /// </summary>
        public void MarkAsUsed(Guid scannedBy, string scanLocation)
        {
            // Idempotency - if already used, do nothing
            if (Status == TicketStatus.Used)
                return;

            if (Status == TicketStatus.Cancelled)
                throw new InvalidOperationException("Cannot use a cancelled ticket");

            if (Status == TicketStatus.Expired)
                throw new InvalidOperationException("Cannot use an expired ticket");

            // Check if event has started (allow entry up to 2 hours before)
            var allowedEntryTime = EventStartDate.AddHours(-2);
            if (DateTime.UtcNow < allowedEntryTime)
                throw new InvalidOperationException($"Ticket cannot be used before {allowedEntryTime:yyyy-MM-dd HH:mm}");

            Status = TicketStatus.Used;
            UsedAt = DateTime.UtcNow;
            ScannedBy = scannedBy;
            ScanLocation = scanLocation;
            UpdatedAt = DateTime.UtcNow;

            RaiseDomainEvent(new TicketScannedEvent(
                Id,
                TicketNumber,
                CustomerId,
                EventId,
                scannedBy,
                scanLocation));
        }

        /// <summary>
        /// Cancels the ticket (refund scenario)
        /// </summary>
        public void Cancel(string reason)
        {
            // Idempotency - if already cancelled, do nothing
            if (Status == TicketStatus.Cancelled)
                return;

            if (Status == TicketStatus.Used)
                throw new InvalidOperationException("Cannot cancel a used ticket");

            Status = TicketStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
            UpdatedAt = DateTime.UtcNow;

            RaiseDomainEvent(new TicketCancelledEvent(Id, TicketNumber, reason));
        }

        /// <summary>
        /// Marks ticket as expired (after event end date)
        /// </summary>
        public void MarkAsExpired()
        {
            // Idempotency - if already expired or used, do nothing
            if (Status == TicketStatus.Expired || Status == TicketStatus.Used)
                return;

            if (Status == TicketStatus.Cancelled)
                return; // Cancelled tickets stay cancelled

            Status = TicketStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks ticket as expired (after event end date)
        /// </summary>
        public void EncryptQrCodeData(string data)
        {
            QrCodeData = data; 
        }

        /// <summary>
        /// Validates if ticket can be used
        /// </summary>
        public (bool CanUse, string? Reason) CanBeUsed()
        {
            if (Status == TicketStatus.Used)
                return (false, $"Ticket already used on {UsedAt:yyyy-MM-dd HH:mm}");

            if (Status == TicketStatus.Cancelled)
                return (false, "Ticket has been cancelled");

            if (Status == TicketStatus.Expired)
                return (false, "Ticket has expired");

            // Check if event has started (allow entry up to 2 hours before)
            var allowedEntryTime = EventStartDate.AddHours(-2);
            if (DateTime.UtcNow < allowedEntryTime)
                return (false, $"Entry not allowed before {allowedEntryTime:yyyy-MM-dd HH:mm}");

            // Check if event has ended
            if (DateTime.UtcNow > EventEndDate.AddHours(2))
                return (false, "Event has ended");

            return (true, null);
        }

        private static string GenerateTicketNumber()
        {
            // Format: TKT-YYYYMMDD-XXXXXX (e.g., TKT-20260208-A1B2C3)
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            return $"TKT-{datePart}-{randomPart}";
        }

       

        private static string GenerateQrCodeData(Guid ticketId, string ticketNumber)
        {
            // Format: TICKETID|TICKETNUMBER|TIMESTAMP
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"{ticketId}|{ticketNumber}|{timestamp}";
        }

        private static string GenerateBarcode(Guid ticketId)
        {
            // Simple numeric barcode (first 12 digits of GUID)
            return ticketId.ToString("N")[..12].ToUpperInvariant();
        }

    }
}
