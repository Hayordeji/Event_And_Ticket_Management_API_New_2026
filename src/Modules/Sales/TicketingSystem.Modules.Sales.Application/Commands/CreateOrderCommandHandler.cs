using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
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


        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        public async Task<Result<string>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate items
                if (request.Items == null || !request.Items.Any())
                    return Result.Failure<string>("Order must contain at least one item.");

                // Check for duplicate order (idempotency)
                var orderExists = await _orderRepository.ExistsAsync(
                    request.CustomerId,
                    cancellationToken);

                if (orderExists)
                    return Result.Failure<string>("An active order already exists for this event.");

                // Create order
                var order = Order.Create(
                    customerId: request.CustomerId,
                    request.EventId,
                    customerEmail: request.CustomerEmail,
                    customerName: request.CustomerName
                );



                // Add items
                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                        return Result.Failure<string>("Item quantity must be greater than zero.");

                    if (item.UnitPrice <= 0)
                        return Result.Failure<string>("Item unit price must be greater than zero.");

                    var unitPrice = Money.Create(item.UnitPrice);

                    var newItem = OrderItem.Create(
                        request.EventId,
                        item.TicketTypeId,
                        request.EventName,
                        item.TicketTypeName,
                        unitPrice.Value,
                        item.Quantity,
                        request.eventStartDate,
                        request.VenueName,
                        request.VenueCity
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

                return Result.Success(order.Value.OrderNumber.Value);
            }
            catch (Exception ex)
            {

                throw new Exception($"{ex.Message}... More details {ex.InnerException}");
            }

            
        }
    }
}
