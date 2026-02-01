using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Domain.Repositories;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Queries
{
    internal class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<IEnumerable<OrderResponse>>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetCustomerOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<IEnumerable<OrderResponse>>> Handle(
            GetCustomerOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(
                request.CustomerId,
                cancellationToken);

            var response = orders.Select(MapToOrderResponse).ToList();

            return Result.Success<IEnumerable<OrderResponse>>(response);
        }

        private static OrderResponse MapToOrderResponse(Order order)
        {
            return new OrderResponse(
                Id: order.Id,
                OrderNumber: order.OrderNumber,
                CustomerId: order.CustomerId,
                EventId: order.EventId,
                Status: order.Status,
                SubTotal: order.TotalAmount.Amount,
                ServiceFee: order.PlatformFee.Amount,
                GrandTotal: order.GrandTotal.Amount,
                Currency: order.GrandTotal.Currency,
                CreatedAt: order.CreatedAt,
                PaidAt: order.PaidAt,
                ExpiresAt: order.ExpiresAt,
                Items: order.Items.Select(i => new OrderItemResponse(
                    Id: i.Id,
                    TicketTypeId: i.TicketTypeId,
                    Quantity: i.Quantity,
                    UnitPrice: i.UnitPrice.Amount,
                    TotalPrice: i.Subtotal.Amount
                )).ToList(),
                Payments: order.Payments.Select(p => new PaymentResponse(
                    Id: p.Id,
                    Amount: p.Amount,
                    Currency: p.Currency,
                    Method: p.Method,
                    Status: p.Status,
                    PaymentReference: p.PaymentReference,
                    PaidAt: p.PaidAt
                )).ToList()
            );
        }
    }
}
