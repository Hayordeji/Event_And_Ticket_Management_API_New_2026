using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    internal class GetEventWithTicketTypesQueryHandler : IRequestHandler<GetEventWithTicketTypesQuery, EventDetailResponse>
    {
        private readonly IEventRepository _eventRepository;

        public GetEventWithTicketTypesQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventDetailResponse> Handle(GetEventWithTicketTypesQuery query, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdWithTicketTypesAsync(query.EventId, cancellationToken);

            if (@event == null)
                throw new NotFoundException(nameof(Event), query.EventId);

            var ticketTypes = @event.TicketTypes.Select(t => new TicketTypeResponse(
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

            return new EventDetailResponse(
                @event.Id,
                @event.HostId,
                @event.Name,
                @event.Description,
                new VenueDto(
                    @event.Venue.Name,
                    @event.Venue.Address,
                    @event.Venue.City,
                    @event.Venue.State,
                    @event.Venue.Country,
                    @event.Venue.PostalCode,
                    @event.Venue.Latitude,
                    @event.Venue.Longitude),
                @event.StartDate,
                @event.EndDate,
                @event.ImageUrl,
                @event.Status.ToString(),
                @event.PublishedAt,
                @event.CancelledAt,
                @event.CancellationReason,
                @event.HasSnapshot,
                @event.CurrentSnapshotVersion,
                ticketTypes,
                @event.CreatedAt);
        }
    }
}
