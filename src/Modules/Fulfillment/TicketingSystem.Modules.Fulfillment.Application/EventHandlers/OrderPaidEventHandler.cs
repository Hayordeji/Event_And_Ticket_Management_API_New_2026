using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Fulfillment.Application.Commands;
using TicketingSystem.Modules.Fulfillment.Application.Services;
using TicketingSystem.Modules.Sales.Domain.Events;

namespace TicketingSystem.Modules.Fulfillment.Application.EventHandlers
{
    /// <summary>
    /// Handles OrderPaidEvent from Sales module to generate tickets automatically
    /// This is the bridge between Sales and Fulfillment modules
    /// </summary>
    public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
    {
        private readonly IMediator _mediator;
        private readonly IOrderDataService _orderDataService;
        private readonly ILogger<OrderPaidEventHandler> _logger;

        public OrderPaidEventHandler(
            IMediator mediator,
            IOrderDataService orderDataService,
            ILogger<OrderPaidEventHandler> logger)
        {
            _mediator = mediator;
            _orderDataService = orderDataService;
            _logger = logger;
        }

        public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processing OrderPaidEvent for order {OrderNumber}. Generating tickets...",
                notification.OrderNumber);

            try
            {
                // Fetch complete order data from Sales and Identity modules
                var orderData = await _orderDataService.GetOrderDataAsync(
                    notification.OrderId,
                    cancellationToken);

                if (orderData == null)
                {
                    _logger.LogError(
                        "Cannot generate tickets: Order data not found for OrderId={OrderId}",
                        notification.OrderId);
                    return;
                }

                _logger.LogDebug(
                    "Order data fetched. Customer={Email}, Event={EventName}, Items={ItemCount}",
                    orderData.CustomerEmail, orderData.EventName, orderData.Items.Count);

                // Build GenerateTicketsCommand with complete data
                var command = new GenerateTicketsCommand(
                    OrderId: orderData.OrderId,
                    OrderNumber: orderData.OrderNumber,
                    CustomerId: orderData.CustomerId,
                    CustomerEmail: orderData.CustomerEmail,
                    CustomerFirstName: orderData.CustomerFirstName,
                    CustomerLastName: orderData.CustomerLastName,
                    EventId: orderData.EventId,
                    EventName: orderData.EventName,
                    EventStartDate: orderData.EventStartDate,
                    EventEndDate: orderData.EventEndDate,
                    VenueName: orderData.VenueName,
                    VenueAddress: orderData.VenueAddress,
                    VenueCity: orderData.VenueCity,
                    Items: orderData.Items.Select(i => new GenerateTicketItemDto(
                        TicketTypeId: i.TicketTypeId,
                        TicketTypeName: i.TicketTypeName,
                        Quantity: i.Quantity,
                        UnitPrice: i.UnitPrice,
                        Currency: orderData.Currency
                    )).ToList()
                );

                var result = await _mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Successfully generated {TicketCount} tickets for order {OrderNumber}",
                        result.Value.Count, notification.OrderNumber);

                    // Send ticket delivery email
                    var deliveryCommand = new SendTicketDeliveryCommand(
                        OrderId: orderData.OrderId,
                        OrderNumber: orderData.OrderNumber,
                        CustomerId: orderData.CustomerId,
                        RecipientEmail: orderData.CustomerEmail,
                        RecipientName: $"{orderData.CustomerFirstName} {orderData.CustomerLastName}",
                        TicketIds: result.Value);

                    var deliveryResult = await _mediator.Send(deliveryCommand, cancellationToken);

                    if (deliveryResult.IsSuccess)
                        _logger.LogInformation(
                            "Tickets delivered to {Email} for order {OrderNumber}",
                            orderData.CustomerEmail, notification.OrderNumber);
                    else
                        _logger.LogError(
                            "Ticket delivery failed for order {OrderNumber}. Error={Error}",
                            notification.OrderNumber, deliveryResult.Error);
                }
                else
                {
                    _logger.LogError(
                        "Failed to generate tickets for order {OrderNumber}. Error: {Error}",
                        notification.OrderNumber, result.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing OrderPaidEvent for order {OrderNumber}",
                    notification.OrderNumber);

                // Don't throw - event handlers should be resilient
                // Consider adding to retry queue or dead letter queue
            }
        }
    }
}
