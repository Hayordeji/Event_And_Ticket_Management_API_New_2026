using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.Services;
using TicketingSystem.Modules.Access.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Access.Infrastructure.Services
{
    /// <summary>
    /// Validates tickets by querying Fulfillment module database
    /// Defined as interface in Application layer, implemented in Infrastructure
    /// </summary>
    public class TicketValidationService : ITicketValidationService
    {
        private readonly FulfillmentDbContext _fulfillmentContext;
        private readonly ILogger<TicketValidationService> _logger;

        public TicketValidationService(
            FulfillmentDbContext fulfillmentContext,
            ILogger<TicketValidationService> logger)
        {
            _fulfillmentContext = fulfillmentContext;
            _logger = logger;
        }

        public async Task<TicketValidationResult> ValidateAsync(
            string qrCodeData,
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Validating QR code for EventId={EventId}", eventId);

            // Find ticket by QR code
            var ticket = await _fulfillmentContext.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.QrCodeData == qrCodeData && !t.IsDeleted, cancellationToken);

            // Invalid QR code
            if (ticket == null)
            {
                _logger.LogWarning("Ticket not found for QR code");
                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: null,
                    TicketNumber: null,
                    TicketTypeName: null,
                    CustomerName: null,
                    DenialReason: DenialReason.InvalidTicket,
                    DenialMessage: "Invalid or unrecognized ticket.");
            }

            _logger.LogDebug(
                "Ticket found: {TicketNumber}, Status={Status}, EventId={TicketEventId}",
                ticket.TicketNumber, ticket.Status, ticket.EventId);

            var customerName = $"{ticket.CustomerFirstName} {ticket.CustomerLastName}";

            // Event mismatch
            if (ticket.EventId != eventId)
            {
                _logger.LogWarning(
                    "Event mismatch. Ticket belongs to EventId={TicketEventId}, scanned at EventId={ScanEventId}",
                    ticket.EventId, eventId);

                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.EventMismatch,
                    DenialMessage: "This ticket is not valid for this event.");
            }

            // Cancelled
            if (ticket.Status == TicketStatus.Cancelled)
            {
                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.TicketCancelled,
                    DenialMessage: "This ticket has been cancelled.");
            }

            // Expired
            if (ticket.Status == TicketStatus.Expired || DateTime.UtcNow > ticket.EventEndDate.AddHours(2))
            {
                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.TicketExpired,
                    DenialMessage: "This ticket has expired.");
            }

            // Already used
            if (ticket.Status == TicketStatus.Used)
            {
                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.AlreadyUsed,
                    DenialMessage: $"This ticket was already used on {ticket.UsedAt:yyyy-MM-dd HH:mm}.");
            }

            // Valid
            _logger.LogDebug("Ticket {TicketNumber} is valid", ticket.TicketNumber);

            return new TicketValidationResult(
                IsValid: true,
                TicketId: ticket.Id,
                TicketNumber: ticket.TicketNumber,
                TicketTypeName: ticket.TicketTypeName,
                CustomerName: customerName,
                DenialReason: null,
                DenialMessage: null);
        }
    }
}
