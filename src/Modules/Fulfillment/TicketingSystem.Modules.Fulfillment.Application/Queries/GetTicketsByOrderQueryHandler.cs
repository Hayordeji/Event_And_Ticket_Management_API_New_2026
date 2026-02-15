using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Fulfillment.Application.Queries
{
    public class GetTicketsByOrderQueryHandler : IRequestHandler<GetTicketsByOrderQuery, Result<List<TicketResponse>>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<GetTicketsByOrderQueryHandler> _logger;

        public GetTicketsByOrderQueryHandler(
            ITicketRepository ticketRepository,
            ILogger<GetTicketsByOrderQueryHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
        }

        public async Task<Result<List<TicketResponse>>> Handle(
            GetTicketsByOrderQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving tickets for order {OrderNumber}", request.OrderNumber);

            var tickets = await _ticketRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (!tickets.Any())
            {
                _logger.LogWarning("No tickets found for order {OrderNumber}", request.OrderNumber);
                throw new NotFoundException("Tickets", request.OrderNumber);
            }

            _logger.LogInformation(
                "Found {TicketCount} tickets for order {OrderNumber}",
                tickets.Count, request.OrderNumber);

            var response = tickets.Select(t => new TicketResponse(
                TicketId: t.Id,
                TicketNumber: t.TicketNumber,
                OrderNumber: t.OrderNumber,
                EventName: t.EventName,
                EventStartDate: t.EventStartDate,
                EventEndDate: t.EventEndDate,
                VenueName: t.VenueName,
                VenueAddress: t.VenueAddress,
                VenueCity: t.VenueCity,
                TicketTypeName: t.TicketTypeName,
                CustomerFirstName: t.CustomerFirstName,
                CustomerLastName: t.CustomerLastName,
                PricePaid: t.PricePaid,
                Currency: t.Currency,
                Status: t.Status.ToString(),
                QrCodeData: t.QrCodeData,
                Barcode: t.Barcode,
                UsedAt: t.UsedAt,
                CreatedAt: t.CreatedAt
            )).ToList();

            return Result.Success(response);
        }
    }
}
