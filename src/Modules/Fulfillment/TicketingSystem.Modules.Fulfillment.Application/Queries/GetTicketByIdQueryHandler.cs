using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.DTOs;
using TicketingSystem.Modules.Fulfillment.Domain.Entitites;
using TicketingSystem.Modules.Fulfillment.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Fulfillment.Application.Queries
{
    public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, Result<TicketResponse>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<GetTicketByIdQueryHandler> _logger;

        public GetTicketByIdQueryHandler(
            ITicketRepository ticketRepository,
            ILogger<GetTicketByIdQueryHandler> logger)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
        }

        public async Task<Result<TicketResponse>> Handle(
            GetTicketByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving ticket {TicketId}", request.TicketId);

            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

            if (ticket == null)
            {
                _logger.LogWarning("Ticket {TicketId} not found", request.TicketId);
                throw new NotFoundException(nameof(Ticket), request.TicketId);
            }
           
            if (ticket.CustomerId != request.requesterId)
            {
                return Result.Failure<TicketResponse>("You are not allowed to view another customer ticket");
            }


            _logger.LogInformation(
                "Ticket {TicketNumber} found. Status={Status}, Event={EventName}",
                ticket.TicketNumber, ticket.Status, ticket.EventName);

            var response = new TicketResponse(
                TicketId: ticket.Id,
                TicketNumber: ticket.TicketNumber,
                OrderNumber: ticket.OrderNumber,
                EventName: ticket.EventName,
                EventStartDate: ticket.EventStartDate,
                EventEndDate: ticket.EventEndDate,
                VenueName: ticket.VenueName,
                VenueAddress: ticket.VenueAddress,
                VenueCity: ticket.VenueCity,
                TicketTypeName: ticket.TicketTypeName,
                CustomerFirstName: ticket.CustomerFirstName,
                CustomerLastName: ticket.CustomerLastName,
                PricePaid: ticket.PricePaid,
                Currency: ticket.Currency,
                Status: ticket.Status.ToString(),
                QrCodeData: ticket.QrCodeData,
                Barcode: ticket.Barcode,
                UsedAt: ticket.UsedAt,
                CreatedAt: ticket.CreatedAt
            );

            return Result.Success(response);
        }
    }
}
