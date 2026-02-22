using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Fulfillment.Application.Queries
{
    public class GetCustomerTicketsQueryHandler : IRequestHandler<GetCustomerTicketsQuery, Result<List<TicketResponse>>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<GetCustomerTicketsQueryHandler> _logger;

        public GetCustomerTicketsQueryHandler(
            ITicketRepository ticketRepository,
            ILogger<GetCustomerTicketsQueryHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
        }

        public async Task<Result<List<TicketResponse>>> Handle(
            GetCustomerTicketsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving tickets for customer {CustomerId}", request.CustomerId);

            var tickets = await _ticketRepository.GetByCustomerIdAsync(
                request.CustomerId,
                cancellationToken);

            
           

            _logger.LogInformation(
                "Found {TicketCount} tickets for customer {CustomerId}",
                tickets.Count, request.CustomerId);

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
