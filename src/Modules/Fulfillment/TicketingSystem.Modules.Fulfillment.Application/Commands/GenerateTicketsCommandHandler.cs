using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Fulfillment.Application.Commands
{
    public class GenerateTicketsCommandHandler : IRequestHandler<GenerateTicketsCommand, Result<List<Guid>>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly FulfillmentDbContext _context;
        private readonly ILogger<GenerateTicketsCommandHandler> _logger;
        private readonly IQrCodeEncryptionService _qrEncryptionService; // ← Inject


        public GenerateTicketsCommandHandler(
            ITicketRepository ticketRepository,
            FulfillmentDbContext context,
            ILogger<GenerateTicketsCommandHandler> logger,
            IQrCodeEncryptionService qrEncryptionService)
        {
            _ticketRepository = ticketRepository;
            _context = context;
            _logger = logger;
            _qrEncryptionService = qrEncryptionService;
        }

        public async Task<Result<List<Guid>>> Handle(
            GenerateTicketsCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Generating tickets for order {OrderNumber}. Customer={CustomerEmail}, Event={EventName}",
                request.OrderNumber, request.CustomerEmail, request.EventName);

            try
            {
                // Check if tickets already generated (idempotency)
                var existingTickets = await _ticketRepository.GetByOrderIdAsync(
                    request.OrderId,
                    cancellationToken);

                if (existingTickets.Any())
                {
                    _logger.LogWarning(
                        "Tickets already generated for order {OrderNumber}. Count={TicketCount}",
                        request.OrderNumber, existingTickets.Count);

                    return Result.Success(existingTickets.Select(t => t.Id).ToList());
                }

                var ticketIds = new List<Guid>();

                // Generate tickets for each order item
                foreach (var item in request.Items)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        _logger.LogDebug(
                            "Creating ticket {Index}/{Total} for ticket type {TicketTypeName}",
                            i + 1, item.Quantity, item.TicketTypeName);



                        var ticket = Ticket.Create(
                            orderId: request.OrderId,
                            orderNumber: request.OrderNumber,
                            customerId: request.CustomerId,
                            customerEmail: request.CustomerEmail,
                            customerFirstName: request.CustomerFirstName,
                            customerLastName: request.CustomerLastName,
                            eventId: request.EventId,
                            eventName: request.EventName,
                            eventStartDate: request.EventStartDate,
                            eventEndDate: request.EventEndDate,
                            venueName: request.VenueName,
                            venueAddress: request.VenueAddress,
                            venueCity: request.VenueCity,
                            ticketTypeId: item.TicketTypeId,
                            ticketTypeName: item.TicketTypeName,
                            pricePaid: item.UnitPrice,
                            currency: item.Currency);

                        var encryptedData =  _qrEncryptionService.Encrypt(ticket.QrCodeData);

                        ticket.EncryptQrCodeData(encryptedData);

                        await _ticketRepository.AddAsync(ticket, cancellationToken);
                        ticketIds.Add(ticket.Id);

                        _logger.LogDebug(
                            "Ticket created: {TicketNumber}, QrCode={QrCodeLength} chars",
                            ticket.TicketNumber, ticket.QrCodeData.Length);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully generated {TicketCount} tickets for order {OrderNumber}",
                    ticketIds.Count, request.OrderNumber);

                return Result.Success(ticketIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating tickets for order {OrderNumber}",
                    request.OrderNumber);

                return Result.Failure<List<Guid>>(
                    $"An error occurred while generating tickets: {ex.Message}");
            }
        }

        /// <summary>
        /// Plain-text payload format: "{TicketId}|{TicketNumber}|{UnixTimestampSeconds}"
        /// The timestamp records when the ticket was issued — useful for detecting
        /// unusually old or future-dated QR codes if policy requires it.
        /// </summary>
        private static string BuildQrPayload(Guid ticketId, string ticketNumber)
            => $"{ticketId}|{ticketNumber}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        private static string GenerateTicketNumber()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            return $"TKT-{datePart}-{randomPart}";
        }

        private static string GenerateBarcode(Guid ticketId)
            => ticketId.ToString("N")[..12].ToUpperInvariant();
    }
}
