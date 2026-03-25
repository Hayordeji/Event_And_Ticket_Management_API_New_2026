using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.Modules.Fulfillment.Application.Services;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Enums;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.Modules.Fulfillment.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Domain.ValueObjects;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.DTOs;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Fulfillment.Application.Commands
{
    public class SendTicketDeliveryCommandHandler : IRequestHandler<SendTicketDeliveryCommand, Result<Guid>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketDeliveryRepository _ticketDeliveryRepository;
        private readonly IPdfTicketGenerator _pdfGenerator;
        private readonly IEmailService _emailService;
        private readonly FulfillmentDbContext _context;
        private readonly ILogger<SendTicketDeliveryCommandHandler> _logger;

        public SendTicketDeliveryCommandHandler(
            ITicketRepository ticketRepository,
            ITicketDeliveryRepository ticketDeliveryRepository,
            IPdfTicketGenerator pdfGenerator,
            IEmailService emailService,
            FulfillmentDbContext context,
            ILogger<SendTicketDeliveryCommandHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _ticketDeliveryRepository = ticketDeliveryRepository;
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            SendTicketDeliveryCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing ticket delivery for order {OrderNumber}. Recipient={Email}",
                request.OrderNumber, request.RecipientEmail);

            // Idempotency: Check if delivery already exists for this order
            var existingDelivery = await _ticketDeliveryRepository.GetByOrderIdAsync(
                request.OrderId,
                cancellationToken);

            if (existingDelivery != null && existingDelivery.Status == DeliveryStatus.Delivered)
            {
                _logger.LogWarning(
                    "Tickets already delivered for order {OrderNumber}. DeliveryId={DeliveryId}",
                    request.OrderNumber, existingDelivery.Id);

                return Result.Success(existingDelivery.Id);
            }

            // Create delivery record
            var delivery = existingDelivery ?? TicketDelivery.Create(
                orderId: request.OrderId,
                orderNumber: request.OrderNumber,
                customerId: request.CustomerId,
                recipientEmail: request.RecipientEmail,
                ticketIds: request.TicketIds);

            if (existingDelivery == null)
            {
                await _context.TicketDeliveries.AddAsync(delivery, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            try
            {
                // Fetch all tickets
                var tickets = new List<Ticket>();
                foreach (var ticketId in request.TicketIds)
                {
                    var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
                    if (ticket != null) tickets.Add(ticket);
                }

                if (!tickets.Any())
                    return Result.Failure<Guid>("No tickets found for delivery.");

                // Mark as sending
                delivery.MarkAsSending("Internal");
                await _context.SaveChangesAsync(cancellationToken);

                // Generate PDF with all tickets
                _logger.LogDebug("Generating PDF for {TicketCount} tickets", tickets.Count);
                var pdfBytes = _pdfGenerator.GenerateTicketsPdf(tickets);

                var firstTicket = tickets.First();
                var emailRequest = new SendTicketEmailRequest(
                    RecipientEmail : request.RecipientEmail,
                    RecipientName : request.RecipientName,
                    OrderNumber : request.OrderNumber,
                    EventName : firstTicket.EventName,
                    EventDate : firstTicket.EventStartDate,
                    VenueName : firstTicket.VenueName,
                    TicketCount : tickets.Count,
                    PdfAttachment : pdfBytes

                );
                
                // Send email
                var result = await _emailService.SendTicketEmailAsync(emailRequest, cancellationToken: cancellationToken);
                    
                if (result.IsSuccess)
                {
                    delivery.MarkAsSent(result.MessageId, result.Response);
                    delivery.MarkAsDelivered();

                    _logger.LogInformation(
                        "Tickets delivered for order {OrderNumber}. MessageId={MessageId}",
                        request.OrderNumber, result.MessageId);
                }
                else
                {
                    delivery.MarkAsFailed(result.Response);

                    _logger.LogError(
                        "Failed to deliver tickets for order {OrderNumber}. Reason={Reason}",
                        request.OrderNumber, result.Response);
                }

                await _context.SaveChangesAsync(cancellationToken);

                return result.IsSuccess
                    ? Result.Success(delivery.Id)
                    : Result.Failure<Guid>($"Email delivery failed: {result.Response}");
            }
            catch (Exception ex)
            {
                delivery.MarkAsFailed(ex.Message);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Error delivering tickets for order {OrderNumber}",
                    request.OrderNumber);

                return Result.Failure<Guid>($"Delivery error: {ex.Message}");
            }
        }
    }
}
