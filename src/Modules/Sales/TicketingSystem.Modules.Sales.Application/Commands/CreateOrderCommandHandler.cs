using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Domain.Entities;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<string>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SalesDbContext _context;
        private readonly IEventValidationService _eventValidationService;
        private readonly ILogger<CreateOrderCommandHandler> _logger;


        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context,
            IEventValidationService eventValidationService, ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _context = context;
            _eventValidationService = eventValidationService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating order for customer {CustomerId} for event {EventId} with {ItemCount} items",request.CustomerId, request.EventId, request.Items.Count);

                // Check for duplicate order (idempotency)
                var orderExists = await _orderRepository.ExistsAsync(
                    request.CustomerId,
                    cancellationToken);

                if (orderExists)
                {
                    _logger.LogWarning(
                    "Customer {CustomerId} already has an active order for event {EventId}",request.CustomerId, request.EventId);
                    return Result.Failure<string>("An active order already exists for this event.");
                }

                // Step 2: Prepare order items for validation
                var orderItems = request.Items.Select(i => new OrderItemValidationDto
                {
                    TicketTypeId = i.TicketTypeId,
                    RequestedQuantity = i.Quantity,
                    SubmittedUnitPrice = i.UnitPrice
                }).ToList();

                // Step 3: Get event data for snapshot
                var eventResult = await _eventValidationService.ValidateEventAsync(request.EventId, cancellationToken);

                if (!eventResult.IsValid || eventResult.EventData == null)
                {
                    _logger.LogError(
                        "Event validation failed unexpectedly after order items validation for event {EventId}",
                        request.EventId);

                    return Result.Failure<string>("Event validation failed unexpectedly");
                }

                // Step 4: Comprehensive validation (capacity, pricing, availability)
                var (isValid, errors) = await _eventValidationService.ValidateOrderItemsAsync(
                    request.EventId,
                    orderItems,
                    cancellationToken);

                if (!isValid)
                {
                    _logger.LogWarning(
                        "Order validation failed for customer {CustomerId} and event {EventId}. Errors: {Errors}",
                        request.CustomerId, request.EventId, string.Join("; ", errors));

                    return Result.Failure<string>(string.Join(Environment.NewLine, errors));
                }


                

                var eventData = eventResult.EventData;

                // Step 5: Create order with event snapshot
                _logger.LogInformation(
                    "Creating order with event snapshot: Event={EventName}, Venue={VenueName}, Date={EventDate}",
                    eventData.EventName, eventData.VenueName, eventData.EventStartDate);


                var ticketTypeIds = request.Items.Select(i => i.TicketTypeId).ToList();

                //var validationResult = await _eventValidationService.ValidateEventAndTicketTypesAsync(
                //    request.EventId,
                //    ticketTypeIds,
                //    cancellationToken);

                //if (!validationResult.IsSuccess)
                //    return Result.Failure<string>(validationResult.Error);

                //if (!validationResult.Value.IsValid)
                //    return Result.Failure<string>(validationResult.Value.ErrorMessage!);

                

                // Create order
                var order = Order.Create(
                    customerId: request.CustomerId,
                    request.EventId,
                    customerEmail: request.CustomerEmail,
                    customerName: request.CustomerName,
                    eventName: eventData.EventName,
                    eventDescription: eventData.EventDescription,
                    eventStartDate: eventData.EventStartDate,
                    eventEndDate: eventData.EventEndDate,
                    venueName: eventData.VenueName,
                    venueAddress: eventData.VenueAddress,
                    venueCity: eventData.VenueCity
                );



                // Add items
                foreach (var item in request.Items)
                {
                    var ticketType = eventData.TicketTypes.First(tt => tt.TicketTypeId == item.TicketTypeId);

                    if (item.Quantity <= 0)
                        return Result.Failure<string>("Item quantity must be greater than zero.");

                    if (item.UnitPrice <= 0)
                        return Result.Failure<string>("Item unit price must be greater than zero.");

                    var unitPrice = Money.Create(item.UnitPrice);

                    var newItem = OrderItem.Create(
                        request.EventId,
                        item.TicketTypeId,
                        ticketType.Name,
                        ticketType.Description,
                        unitPrice.Value,
                        item.Quantity
                    );

                    order.Value.AddItem(
                        newItem.Value
                    );
                }

                // Recalculate totals
                order.Value.RecalculateTotals();

                // Save
                await _orderRepository.AddAsync(order.Value, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
               "Order {OrderNumber} created successfully for customer {CustomerId}. OrderId={OrderId}, Total={GrandTotal} {Currency}",
               order.Value.OrderNumber, request.CustomerId, order.Value.Id, order.Value.GrandTotal, order.Value.TotalAmount.Currency);

                return Result.Success(order.Value.OrderNumber.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"An unexpected error occured while creating order for customer {CustomerId} and event {EventId}",request.CustomerId, request.EventId);
                throw new Exception($"{ex.Message}... More details {ex.InnerException}");
            }

            
        }
    }
}
