using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;
using TicketingSystem.SharedKernel.Services;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SalesDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(
            CancelOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Get order
            var order = await _orderRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (order == null)
                throw new NotFoundException(nameof(Order), request.OrderNumber);

            // Ownership check: Allow if customer owns order OR user is admin
            if (order.CustomerId != _currentUserService.UserId && !_currentUserService.IsAdmin())
                throw new ForbiddenException("You can only cancel your own orders");

            // Validate cancellation reason
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                return Result.Failure("Cancellation reason is required.");

            // Validate order status
            if (order.Status == Domain.Enums.OrderStatus.Cancelled)
                return Result.Failure("Order is already cancelled.");

            if (order.Status == Domain.Enums.OrderStatus.Fulfilled)
                return Result.Failure("Cannot cancel a fulfilled order.");

            if (order.Status == Domain.Enums.OrderStatus.Refunded)
                return Result.Failure("Cannot cancel a refunded order.");

            // Cancel order
            order.Cancel(request.CancellationReason);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
