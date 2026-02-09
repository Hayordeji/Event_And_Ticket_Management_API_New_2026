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
    public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand, Result>
    {
        private readonly IEventRepository _eventRepository;
        private readonly CatalogDbContext _context;

        public CancelEventCommandHandler(
            IEventRepository eventRepository,
            CatalogDbContext context)
        {
            _eventRepository = eventRepository;
            _context = context;
        }

        public async Task<Result> Handle(CancelEventCommand command, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(command.EventId, cancellationToken);

            if (@event == null)
                throw new NotFoundException(nameof(Event), command.EventId);

            // Authorization check
            if (@event.HostId != command.HostId)
                throw new ForbiddenException("You can only cancel your own events");

            // Cancel
            var cancelResult = @event.Cancel(command.Reason);

            if (!cancelResult.IsSuccess)
                return cancelResult;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
