using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.Modules.Catalog.Domain.ValueObjects;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<Guid>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly CatalogDbContext _context;

        public CreateEventCommandHandler(
            IEventRepository eventRepository,
            CatalogDbContext context)
        {
            _eventRepository = eventRepository;
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreateEventCommand command, CancellationToken cancellationToken)
        {
            // Create venue value object
            var venueResult = Venue.Create(
                command.Request.Venue.Name,
                command.Request.Venue.Address,
                command.Request.Venue.City,
                command.Request.Venue.State,
                command.Request.Venue.Country,
                command.Request.Venue.PostalCode,
                command.Request.Venue.Latitude,
                command.Request.Venue.Longitude);

            if (!venueResult.IsSuccess)
                return Result.Failure<Guid>(venueResult.Error);

            // Create event aggregate
            var eventResult = Event.Create(
                command.HostId,
                command.Request.Name,
                command.Request.Description,
                venueResult.Value,
                command.Request.StartDate,
                command.Request.EndDate,
                command.Request.ImageUrl);

            if (!eventResult.IsSuccess)
                return Result.Failure<Guid>(eventResult.Error);

            var @event = eventResult.Value;

            // Persist
            await _eventRepository.AddAsync(@event, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(@event.Id);
        }
    }
}
