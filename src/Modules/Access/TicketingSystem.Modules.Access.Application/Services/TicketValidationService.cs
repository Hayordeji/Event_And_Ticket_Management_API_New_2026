using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Access.Application.Services;
using TicketingSystem.Modules.Access.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;
using TicketingSystem.SharedKernel.Services;

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
        private readonly IQrCodeEncryptionService _qrEncryptionService; // ← Inject


        public TicketValidationService(
            FulfillmentDbContext fulfillmentContext,
            ILogger<TicketValidationService> logger,
            IQrCodeEncryptionService qrEncryptionService)
        {
            _fulfillmentContext = fulfillmentContext;
            _logger = logger;
            _qrEncryptionService = qrEncryptionService;
        }

        public async Task<TicketValidationResult> ValidateAsync(
            string qrCodeData,
            CancellationToken cancellationToken = default)
        {


            // ── Step 1: Decrypt and authenticate the QR payload ──────────────────────
            var decryptResult = _qrEncryptionService.Decrypt(qrCodeData);

            if (decryptResult.IsFailure)
            {
                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: null,
                    EventId: null,
                    TicketNumber: null,
                    TicketTypeName: null,
                    CustomerName: null,
                    DenialReason: DenialReason.InvalidTicket,
                    DenialMessage: "QR code could not be verified.It may be counterfeit or damaged.");

               
            }

            // ── Step 2: Parse the decrypted payload ───────────────────────────────────
            var parts = decryptResult.Value.Split('|');

            if (parts.Length != 3
                || !Guid.TryParse(parts[0], out var ticketId)
                || string.IsNullOrWhiteSpace(parts[1]))
            {

                return new TicketValidationResult(
                    IsValid: false,
                    TicketId: null,
                    EventId: null,
                    TicketNumber: null,
                    TicketTypeName: null,
                    CustomerName: null,
                    DenialReason: DenialReason.InvalidTicket,
                    DenialMessage: "QR code payload is malformed.");

                
            }

            var ticketNumber = parts[1];

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
                    EventId: null,
                    TicketNumber: ticketNumber,
                    TicketTypeName: null,
                    CustomerName: null,
                    DenialReason: DenialReason.InvalidTicket,
                    DenialMessage: "Invalid or unrecognized ticket.");
            }

            _logger.LogDebug(
                "Ticket found: {TicketNumber}, Status={Status}, EventId={TicketEventId}",
                ticket.TicketNumber, ticket.Status, ticket.EventId);

            var customerName = $"{ticket.CustomerFirstName} {ticket.CustomerLastName}";

           


            switch (ticket.Status)
            {
                case TicketStatus.Valid:
                    _logger.LogDebug("Ticket {TicketNumber} is valid", ticket.TicketNumber);

                    return new TicketValidationResult(
                        IsValid: true,
                        TicketId: ticket.Id,
                        EventId: ticket.EventId,
                        TicketNumber: ticket.TicketNumber,
                        TicketTypeName: ticket.TicketTypeName,
                        CustomerName: customerName,
                        DenialReason: null,
                        DenialMessage: null);

                case TicketStatus.Used:
                    return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    EventId: ticket.EventId,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.AlreadyUsed,
                    DenialMessage: $"This ticket was already used on {ticket.UsedAt:yyyy-MM-dd HH:mm}.");

                case TicketStatus.Cancelled:

                    return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    EventId: ticket.EventId,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.TicketCancelled,
                    DenialMessage: "This ticket has been cancelled.");

                case TicketStatus.Expired:
                    return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    EventId: ticket.EventId,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.TicketExpired,
                    DenialMessage: "This ticket has expired.");

                default:
                    return new TicketValidationResult(
                    IsValid: false,
                    TicketId: ticket.Id,
                    EventId: ticket.EventId,
                    TicketNumber: ticket.TicketNumber,
                    TicketTypeName: ticket.TicketTypeName,
                    CustomerName: customerName,
                    DenialReason: DenialReason.InvalidTicket,
                    DenialMessage: "This ticket does not have any status.");
            }
           
        }
    }
}
