using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Catalog.Application.Services;
using TicketingSystem.Modules.Catalog.Domain.Events;
using TicketingSystem.Modules.Catalog.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;


namespace TicketingSystem.Modules.Catalog.Application.EventHandlers
{
    public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
    {
        private readonly IMediator _mediator;
        private readonly IOrderDataService _orderDataService;
        private readonly CatalogDbContext _context;
        private readonly ILogger<OrderPaidEventHandler> _logger;

        public OrderPaidEventHandler(
            IMediator mediator,
            IOrderDataService orderDataService,
            ILogger<OrderPaidEventHandler> logger,
            CatalogDbContext context)
        {
            _mediator = mediator;
            _orderDataService = orderDataService;
            _logger = logger;
            _context = context;
        }



        public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch complete order data from Sales and Identity modules
                var orderData = await _orderDataService.GetOrderDataAsync(
                    notification.OrderId,
                    cancellationToken);


                if (orderData == null)
                {
                    _logger.LogError(
                        "Cannot add ticket sold counts: Order data not found for OrderId={OrderId}",
                        notification.OrderId);

                    throw new Exception($"Cannot add ticket sold counts:  Order data not found for OrderId={notification.OrderId}");
                }

                _logger.LogDebug(
                    "Order data fetched. Customer={Email}, Event={EventName}, Items={ItemCount}",
                    orderData.CustomerEmail, orderData.EventName, orderData.Items.Count);


                //GET THE TICKETS DATA FROM THE ORDER_TICKETID 
                foreach (var ticket in orderData.Items)
                {
                    var dbTicket = await _context.TicketTypes.FirstOrDefaultAsync(
                        t => t.Id == ticket.TicketTypeId);

                    if (dbTicket == null)
                    {
                        _logger.LogError(
                       "Cannot add ticket sold counts: Ticket Type data not found for TicketId={TicketId}",
                       ticket.TicketTypeId);
                         
                        throw new Exception($"Cannot add ticket sold counts:   Ticket Type data not found for TicketId={ticket.TicketTypeId}");
                    }

                    var result = dbTicket.MarkAsSold(ticket.Quantity);
                    if (!result.IsSuccess)
                    {
                        _logger.LogError(
                        result.Error,
                        ticket.TicketTypeId);

                        throw new Exception($"Cannot add ticket sold counts: {result.Error}");
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing OrderPaidEvent for order {OrderNumber}",
                    notification.OrderNumber);

                throw new Exception($"\"Error processing OrderPaidEvent for order {notification.OrderNumber}");

            }



        }

    }
}
