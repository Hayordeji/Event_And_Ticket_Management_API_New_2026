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
    public class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SalesDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RequestRefundCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _context = context;
            _currentUserService = currentUserService;
        }


        public async Task<Result> Handle(
           RequestRefundCommand request,
           CancellationToken cancellationToken)
        {
            // Get order
            var order = await _orderRepository.GetByOrderNumberAsync(
                request.OrderNumber,
                cancellationToken);

            if (order == null)
                throw new NotFoundException(nameof(Order), request.OrderNumber);

            // Cancel order
            var result =  order.Refund(request.Reason);

            // Ownership check: Only allow order owner or admin to request refund
            if (order.CustomerId != _currentUserService.UserId && !_currentUserService.IsAdmin())
                throw new ForbiddenException("You can only request refund for your own orders");

            if (result.IsFailure)
                return result;

            await _context.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
