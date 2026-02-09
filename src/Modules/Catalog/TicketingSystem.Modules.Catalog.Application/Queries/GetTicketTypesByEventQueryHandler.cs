using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    public class GetTicketTypesByEventQueryHandler : IRequestHandler<GetTicketTypesByEventQuery, Result<List<TicketTypeResponse>>>
    {
        private readonly IEventRepository _eventRepository;

        public GetTicketTypesByEventQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Result<List<TicketTypeResponse>>> Handle(
            GetTicketTypesByEventQuery query,
            CancellationToken cancellationToken)
        {
            var result = new List<TicketTypeResponse>();

            // Verify event exists
            var eventExists = await _eventRepository.ExistsAsync(query.EventId, cancellationToken);
            if (!eventExists)
                throw new NotFoundException(nameof(Event), query.EventId);

            // Get ticket types through event repository
            var ticketTypes = await _eventRepository.GetTicketTypesByEventIdAsync(
                query.EventId,
                cancellationToken);

            result = ticketTypes.Select(t => new TicketTypeResponse(
                t.Id,
                t.EventId,
                t.Name,
                t.Description,
                t.Price.Amount,
                t.Price.Currency,
                t.TotalCapacity,
                t.SoldCount,
                t.ReservedCount,
                t.AvailableCount,
                t.IsSoldOut,
                t.SaleStartDate,
                t.SaleEndDate,
                t.MinPurchaseQuantity,
                t.MaxPurchaseQuantity,
                t.IsActive)).ToList();


            return Result<List<TicketTypeResponse>>.Success(result);
            //return Result<List<TicketTypeResponse>>.Success(ticketTypes.Select(t => new TicketTypeResponse(
            //    t.Id,
            //    t.EventId,
            //    t.Name,
            //    t.Description,
            //    t.Price.Amount,
            //    t.Price.Currency,
            //    t.TotalCapacity,
            //    t.SoldCount,
            //    t.ReservedCount,
            //    t.AvailableCount,
            //    t.IsSoldOut,
            //    t.SaleStartDate,
            //    t.SaleEndDate,
            //    t.MinPurchaseQuantity,
            //    t.MaxPurchaseQuantity,
            //    t.IsActive)).ToList());


        }
    }
}
