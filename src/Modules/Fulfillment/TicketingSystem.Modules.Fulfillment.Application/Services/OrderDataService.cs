using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;

namespace TicketingSystem.Modules.Fulfillment.Application.Services
{
    public class OrderDataService : IOrderDataService
    {
        private readonly SalesDbContext _salesContext;
        private readonly IdentityDbContext _identityContext;
        private readonly ILogger<OrderDataService> _logger;

        public OrderDataService(
            SalesDbContext salesContext,
            IdentityDbContext identityContext,
            ILogger<OrderDataService> logger)
        {
            _salesContext = salesContext;
            _identityContext = identityContext;
            _logger = logger;
        }

        public async Task<OrderDataDto?> GetOrderDataAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching order data for OrderId={OrderId}", orderId);

            var order = await _salesContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return null;
            }

            // Get customer details
            var customer = await _identityContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == order.CustomerId && !u.IsDeleted, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for order {OrderId}", order.CustomerId, orderId);
                return null;
            }

            _logger.LogDebug(
                "Order data fetched successfully. OrderNumber={OrderNumber}, CustomerEmail={Email}, ItemCount={ItemCount}",
                order.OrderNumber, customer.Email, order.Items.Count);

            return new OrderDataDto(
                OrderId: order.Id,
                OrderNumber: order.OrderNumber,
                CustomerId: customer.Id,
                CustomerEmail: customer.Email,
                CustomerFirstName: customer.FirstName,
                CustomerLastName: customer.LastName,
                EventId: order.EventId,
                EventName: order.EventName,
                EventDescription: order.EventDescription,
                EventStartDate: order.EventStartDate,
                EventEndDate: order.EventEndDate,
                VenueName: order.VenueName,
                VenueAddress: order.VenueAddress,
                VenueCity: order.VenueCity,
                Currency: order.TotalAmount.Currency,
                Items: order.Items.Select(i => new OrderItemDataDto(
                    TicketTypeId: i.TicketTypeId,
                    TicketTypeName: i.TicketTypeName,
                    TicketTypeDescription: i.TicketTypeDescription,
                    Quantity: i.Quantity,
                    UnitPrice: i.UnitPrice.Amount
                )).ToList()
            );
        }

        public async Task<OrderDataDto?> GetOrderDataByNumberAsync(
            string orderNumber,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching order data for OrderNumber={OrderNumber}", orderNumber);

            var order = await _salesContext.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderNumber} not found", orderNumber);
                return null;
            }

            return await GetOrderDataAsync(order.Id, cancellationToken);
        }
    }
}
