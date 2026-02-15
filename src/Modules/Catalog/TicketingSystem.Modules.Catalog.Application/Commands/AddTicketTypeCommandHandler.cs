using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Catalog.Domain.Repositories;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Catalog.Application.Commands
{
    public class AddTicketTypeCommandHandler : IRequestHandler<AddTicketTypeCommand, Result<Guid>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly CatalogDbContext _context;

        public AddTicketTypeCommandHandler(
            IEventRepository eventRepository,
            CatalogDbContext context)
        {
            _eventRepository = eventRepository;
            _context = context;
        }

        public async Task<Result<Guid>> Handle(AddTicketTypeCommand command, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdWithTicketTypesAsync(command.EventId, cancellationToken);

            if (@event == null)
                throw new NotFoundException(nameof(Event), command.EventId);

            // Authorization check
            if (@event.HostId != command.HostId)
                throw new ForbiddenException("You can only add ticket types to your own events");

            if (@event.TicketTypes.Count() >= 5)
                return Result.Failure<Guid>("You can not create more than 5 ticket types");

            // Create Money value object
            var priceResult = Money.Create(command.Request.Price, command.Request.Currency);

            if (!priceResult.IsSuccess)
                return Result.Failure<Guid>(priceResult.Error);

            // Add ticket type through aggregate
            var ticketTypeResult = @event.AddTicketType(
                command.Request.Name,
                priceResult.Value,
                command.Request.TotalCapacity,
                command.Request.Description,
                command.Request.SaleStartDate,
                command.Request.SaleEndDate,
                command.Request.MinPurchaseQuantity,
                command.Request.MaxPurchaseQuantity);

            if (!ticketTypeResult.IsSuccess)
                return Result.Failure<Guid>(ticketTypeResult.Error);

            var ticketType = ticketTypeResult.Value;
            await _context.TicketTypes.AddAsync(ticketType, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(ticketType.Id);
        }
    }
}
