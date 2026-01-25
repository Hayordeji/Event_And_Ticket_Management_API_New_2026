using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.Modules.Catalog.Domain.ValueObjects;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result>
    {
        private readonly IEventRepository _eventRepository;
        private readonly CatalogDbContext _context;

        public UpdateEventCommandHandler(
            IEventRepository eventRepository,
            CatalogDbContext context)
        {
            _eventRepository = eventRepository;
            _context = context;
        }

        public async Task<Result> Handle(UpdateEventCommand command, CancellationToken cancellationToken)
        {
            // Load event with ticket types (to check if tickets sold)
            var @event = await _eventRepository.GetByIdWithTicketTypesAsync(command.EventId, cancellationToken);

            if (@event == null)
                throw new NotFoundException(nameof(Event), command.EventId);

            // Authorization check
            if (@event.HostId != command.HostId)
                throw new ForbiddenException("You can only update your own events");

            // Parse venue if provided
            Venue? venue = null;
            if (command.Request.Venue != null)
            {
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
                    return Result.Failure(venueResult.Error);

                venue = venueResult.Value;
            }

            // Update event (will create snapshot if needed)
            var updateResult = @event.Update(
                command.Request.Name,
                command.Request.Description,
                venue,
                command.Request.StartDate,
                command.Request.EndDate,
                command.Request.ImageUrl);

            if (!updateResult.IsSuccess)
                return updateResult;

            // Persist
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
