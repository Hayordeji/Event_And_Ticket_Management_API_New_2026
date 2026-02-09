using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public class PublishEventCommandHandler :  IRequestHandler<PublishEventCommand, Result>
    {
        private readonly IEventRepository _eventRepository;
        private readonly CatalogDbContext _context;

        public PublishEventCommandHandler(
            IEventRepository eventRepository,
            CatalogDbContext context)
        {
            _eventRepository = eventRepository;
            _context = context;
        }

        public async Task<Result> Handle(PublishEventCommand command, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdWithTicketTypesAsync(command.EventId, cancellationToken);

            if (@event == null)
                throw new NotFoundException(nameof(Event), command.EventId);

            // Authorization check
            if (@event.HostId != command.HostId)
                throw new ForbiddenException("You can only publish your own events");

            // Publish
            var publishResult = @event.Publish();

            if (!publishResult.IsSuccess)
                return publishResult;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
