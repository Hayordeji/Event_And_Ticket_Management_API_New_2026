using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.DTOs;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Queries
{
    /// <summary>
    /// Handler for searching Host's events with filters and pagination
    /// </summary>
    public class SearchHostEventsQueryCommandHandler 
        : IRequestHandler<SearchHostEventsQueryCommand, Result<(List<EventResponse> Events, int TotalCount)>>
    {
        private readonly IEventRepository _eventRepository;

        public SearchHostEventsQueryCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Result<(List<EventResponse> Events, int TotalCount)>> Handle(
            SearchHostEventsQueryCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                // The repository returns a tuple of (Events, TotalCount)
                var (events, totalCount) = await _eventRepository.SearchHostEventsAsync(
                    command.Request,
                    command.HostId,
                    cancellationToken);

                var eventResponses = events.Select(e => new EventResponse(
                    e.Id,
                    e.HostId,
                    e.Name,
                    e.Description,
                    new VenueDto(
                        e.Venue.Name,
                        e.Venue.Address,
                        e.Venue.City,
                        e.Venue.State,
                        e.Venue.Country,
                        e.Venue.PostalCode,
                        e.Venue.Latitude,
                        e.Venue.Longitude),
                    e.StartDate,
                    e.EndDate,
                    e.ImageUrl,
                    e.Status.ToString(),
                    e.PublishedAt,
                    e.CancelledAt,
                    e.CancellationReason,
                    e.HasSnapshot,
                    e.CurrentSnapshotVersion,
                    e.CreatedAt)).ToList();

                return Result.Success((eventResponses, totalCount));
            }
            catch (Exception ex)
            {
                return Result.Failure<(List<EventResponse> Events, int TotalCount)>(
                    $"An error occurred while searching events: {ex.Message}");
            }
        }
    }
}
