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
    internal class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventResponse>
    {
        private readonly IEventRepository _eventRepository;

        public GetEventByIdQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventResponse> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(query.EventId, cancellationToken);

            if (@event == null)
                throw new NotFoundException(nameof(Event), query.EventId);

            return new EventResponse(
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
                @event.CreatedAt);
        }
    }
}
