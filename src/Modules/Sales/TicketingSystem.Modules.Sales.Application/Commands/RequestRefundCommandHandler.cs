using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel;
using TicketingSystem.SharedKernel.Exceptions;

namespace TicketingSystem.Modules.Sales.Application.Commands
{
    public class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SalesDbContext _context;

        public RequestRefundCommandHandler(
            IOrderRepository orderRepository,
            SalesDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
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

            return result;
        }
    }
}
