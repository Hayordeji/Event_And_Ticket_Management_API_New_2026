using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services
{
    public class EventValidationService : IEventValidationService
    {
        private readonly CatalogDbContext _catalogContext;
        private readonly SalesDbContext _salesContext;
        private readonly ILogger<EventValidationService> _logger;
        public EventValidationService(
        CatalogDbContext catalogContext,
        SalesDbContext salesContext,
        ILogger<EventValidationService> logger)
        {
            _catalogContext = catalogContext;
            _salesContext = salesContext;
            _logger = logger;
        }

        public async Task<EventValidationResult> ValidateEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Validating event {EventId}", eventId);

            var eventEntity = await _catalogContext.Events
                .Include(e => e.TicketTypes)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted, cancellationToken);

            if (eventEntity == null)
            {
                _logger.LogWarning("Event {EventId} not found", eventId);
                return EventValidationResult.Failure($"Event with ID {eventId} not found");
            }

            if (eventEntity.IsCancelled)
            {
                _logger.LogWarning("Event {EventId} is cancelled", eventId);
                return EventValidationResult.Failure($"Event '{eventEntity.Name}' has been cancelled");
            }

            // Get sold counts for all ticket types
            var ticketTypeIds = eventEntity.TicketTypes.Select(tt => tt.Id).ToList();
            var soldCounts = await GetSoldCountsForTicketTypesAsync(ticketTypeIds, cancellationToken);

            var eventData = new EventValidationDto
            {
                EventId = eventEntity.Id,
                EventName = eventEntity.Name,
                EventDescription = eventEntity.Description,
                EventStartDate = eventEntity.StartDate,
                EventEndDate = eventEntity.EndDate.Value,
                VenueName = eventEntity.Venue?.Name ?? string.Empty,
                VenueAddress = eventEntity.Venue?.Address ?? string.Empty,
                VenueCity = eventEntity.Venue?.City ?? string.Empty,
                IsCancelled = eventEntity.IsCancelled,
                TicketTypes = eventEntity.TicketTypes.Select(tt => new TicketTypeValidationDto
                {
                    TicketTypeId = tt.Id,
                    Name = tt.Name,
                    Description = tt.Description,
                    Price = tt.Price.Amount,
                    Currency = tt.Price.Currency,
                    TotalCapacity = tt.TotalCapacity,
                    SoldCount = soldCounts.GetValueOrDefault(tt.Id, 0),
                    IsActive = tt.IsActive,
                    SaleStartDate = tt.SaleStartDate,
                    SaleEndDate = tt.SaleEndDate
                }).ToList()
            };

            _logger.LogInformation(
                "Event {EventId} validated successfully. TicketTypes: {TicketTypeCount}",
                eventId, eventData.TicketTypes.Count);

            return EventValidationResult.Success(eventData);
        }

        public async Task<EventValidationResult> ValidateEventWithTicketTypesAsync(
            Guid eventId,
            List<Guid> ticketTypeIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Validating event {EventId} with {TicketTypeCount} ticket types",
                eventId, ticketTypeIds.Count);

            var eventResult = await ValidateEventAsync(eventId, cancellationToken);

            if (!eventResult.IsValid || eventResult.EventData == null)
                return eventResult;

            var errors = new List<string>();
            var eventData = eventResult.EventData;

            foreach (var ticketTypeId in ticketTypeIds)
            {
                var ticketType = eventData.TicketTypes.FirstOrDefault(tt => tt.TicketTypeId == ticketTypeId);

                if (ticketType == null)
                {
                    var error = $"Ticket type {ticketTypeId} not found for event '{eventData.EventName}'";
                    errors.Add(error);
                    _logger.LogWarning(error);
                }
            }

            if (errors.Any())
                return EventValidationResult.Failure(errors.ToArray());

            _logger.LogInformation(
                "Event {EventId} and ticket types validated successfully",
                eventId);

            return EventValidationResult.Success(eventData);
        }

        public async Task<(bool IsValid, List<string> Errors)> ValidateOrderItemsAsync(
            Guid eventId,
            List<OrderItemValidationDto> items,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Validating {ItemCount} order items for event {EventId}",
                items.Count, eventId);

            var errors = new List<string>();

            if (!items.Any())
            {
                errors.Add("Order must contain at least one item");
                _logger.LogWarning("Order validation failed: No items provided");
                return (false, errors);
            }

            // Get event with ticket types
            var ticketTypeIds = items.Select(i => i.TicketTypeId).Distinct().ToList();
            var eventResult = await ValidateEventWithTicketTypesAsync(eventId, ticketTypeIds, cancellationToken);

            if (!eventResult.IsValid || eventResult.EventData == null)
            {
                _logger.LogWarning("Event validation failed for event {EventId}", eventId);
                return (false, eventResult.Errors);
            }

            var eventData = eventResult.EventData;

            // Validate each order item
            foreach (var item in items)
            {
                var ticketType = eventData.TicketTypes.First(tt => tt.TicketTypeId == item.TicketTypeId);

                // 1. Check if ticket type is active for sale
                if (!ticketType.IsSaleActive)
                {
                    var error = $"Ticket type '{ticketType.Name}' is not currently available for sale";
                    errors.Add(error);
                    _logger.LogWarning(
                        "Ticket type {TicketTypeId} not available for sale. IsActive: {IsActive}, SaleStartDate: {SaleStartDate}, SaleEndDate: {SaleEndDate}",
                        ticketType.TicketTypeId, ticketType.IsActive, ticketType.SaleStartDate, ticketType.SaleEndDate);
                    continue;
                }

                // 2. Check if sold out
                if (ticketType.IsSoldOut)
                {
                    var error = $"Ticket type '{ticketType.Name}' is sold out";
                    errors.Add(error);
                    _logger.LogWarning(
                        "Ticket type {TicketTypeId} is sold out. Capacity: {Capacity}, Sold: {Sold}",
                        ticketType.TicketTypeId, ticketType.TotalCapacity, ticketType.SoldCount);
                    continue;
                }

                // 3. Check available capacity
                if (item.RequestedQuantity > ticketType.AvailableCount)
                {
                    var error = $"Ticket type '{ticketType.Name}' has only {ticketType.AvailableCount} tickets available. You requested {item.RequestedQuantity}";
                    errors.Add(error);
                    _logger.LogWarning(
                        "Insufficient capacity for ticket type {TicketTypeId}. Available: {Available}, Requested: {Requested}",
                        ticketType.TicketTypeId, ticketType.AvailableCount, item.RequestedQuantity);
                    continue;
                }

                // 4. Verify pricing (prevent price manipulation)
                if (item.SubmittedUnitPrice != ticketType.Price)
                {
                    var error = $"Price mismatch for ticket type '{ticketType.Name}'. Expected: {ticketType.Price:N2} {ticketType.Currency}, Submitted: {item.SubmittedUnitPrice:N2}";
                    errors.Add(error);
                    _logger.LogWarning(
                        "Price mismatch for ticket type {TicketTypeId}. Expected: {ExpectedPrice}, Submitted: {SubmittedPrice}",
                        ticketType.TicketTypeId, ticketType.Price, item.SubmittedUnitPrice);
                    continue;
                }

                // 5. Validate quantity
                if (item.RequestedQuantity <= 0)
                {
                    var error = $"Quantity must be greater than zero for ticket type '{ticketType.Name}'";
                    errors.Add(error);
                    _logger.LogWarning(
                        "Invalid quantity for ticket type {TicketTypeId}: {Quantity}",
                        ticketType.TicketTypeId, item.RequestedQuantity);
                    continue;
                }

                _logger.LogDebug(
                    "Validated order item: TicketType={TicketTypeId}, Quantity={Quantity}, UnitPrice={UnitPrice}",
                    item.TicketTypeId, item.RequestedQuantity, item.SubmittedUnitPrice);
            }

            var isValid = !errors.Any();

            if (isValid)
            {
                _logger.LogInformation(
                    "All {ItemCount} order items validated successfully for event {EventId}",
                    items.Count, eventId);
            }
            else
            {
                _logger.LogWarning(
                    "Order validation failed with {ErrorCount} errors for event {EventId}",
                    errors.Count, eventId);
            }

            return (isValid, errors);
        }

        public async Task<int> GetTicketTypeSoldCountAsync(
            Guid ticketTypeId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting sold count for ticket type {TicketTypeId}", ticketTypeId);

            var soldCount = await _salesContext.OrderItems
                .Where(oi => oi.TicketTypeId == ticketTypeId && !oi.IsDeleted)
                .Join(
                    _salesContext.Orders.Where(o => o.Status == Domain.Enums.OrderStatus.Paid && !o.IsDeleted),
                    oi => oi.Id,
                    o => o.Id,
                    (oi, o) => oi.Quantity)
                .SumAsync(cancellationToken);

            _logger.LogDebug(
                "Ticket type {TicketTypeId} sold count: {SoldCount}",
                ticketTypeId, soldCount);

            return soldCount;
        }

        private async Task<Dictionary<Guid, int>> GetSoldCountsForTicketTypesAsync(
            List<Guid> ticketTypeIds,
            CancellationToken cancellationToken = default)
        {
            if (!ticketTypeIds.Any())
                return new Dictionary<Guid, int>();

            _logger.LogDebug(
                "Getting sold counts for {TicketTypeCount} ticket types",
                ticketTypeIds.Count);

            var soldCounts = await _salesContext.OrderItems
                .Where(oi => ticketTypeIds.Contains(oi.TicketTypeId) && !oi.IsDeleted)
                .Join(
                    _salesContext.Orders.Where(o => o.Status == Domain.Enums.OrderStatus.Paid && !o.IsDeleted),
                    oi => oi.Id,
                    o => o.Id,
                    (oi, o) => new { oi.TicketTypeId, oi.Quantity })
                .GroupBy(x => x.TicketTypeId)
                .Select(g => new { TicketTypeId = g.Key, SoldCount = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.TicketTypeId, x => x.SoldCount, cancellationToken);

            _logger.LogDebug(
                "Retrieved sold counts for {Count} ticket types",
                soldCounts.Count);

            return soldCounts;
        }

        public async Task<Result<EventValidationResponse>> ValidateEventAndTicketTypesAsync(Guid eventId, List<Guid> ticketTypeIds, CancellationToken cancellationToken = default)
        {
            // Check if event exists
            var eventExists = await _catalogContext.Events
                .AnyAsync(e => e.Id == eventId, cancellationToken);

            if (!eventExists)
                return Result.Success(new EventValidationResponse(
                false,
                $"Event with ID {eventId} does not exist."));

            // Check if all ticket types exist and belong to this event
            foreach (var ticketTypeId in ticketTypeIds)
            {
                var ticketTypeExists = await _catalogContext.TicketTypes
                    .AnyAsync(t => t.Id == ticketTypeId && t.EventId == eventId, cancellationToken);

                if (!ticketTypeExists)
                return Result.Success(new EventValidationResponse(
                false,
                $"Ticket type {ticketTypeId} does not exist or doesn't belong to event {eventId}."));
            }

            return Result.Success(new EventValidationResponse(true));
        }
    }
}
