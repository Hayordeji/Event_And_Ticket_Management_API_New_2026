using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Application.Queries;
using TicketingSystem.SharedKernel.ApiResponses;

namespace TicketingSystem.Modules.Sales.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var customer = GetCurrentUserId();

            var command = new CreateOrderCommand(
                customer.CustomerId,
                request.EventId,
                request.EventName,
                customer.Email,
                customer.Name,
                request.eventStartDate,
                request.VenueName,
                request.VenueCity,
                request.Items
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return CreatedAtAction(
                nameof(GetOrderByNumber),
                new { orderNumber = result.Value },
                ApiResponse<Guid>.SuccessResponse(result.Value, "Order created successfully.")
            );
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("{orderNumber}")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderByNumber(
            string orderNumber,
            CancellationToken cancellationToken)
        {
            var query = new GetOrderByNumberQuery(orderNumber);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse<OrderResponse>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Get all orders for current customer
        /// </summary>
        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
        {
            var customer = GetCurrentUserId();
            var query = new GetCustomerOrdersQuery(customer.CustomerId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(ApiResponse<IEnumerable<OrderResponse>>.SuccessResponse(result.Value));
        }

        /// <summary>
        /// Process payment for an order
        /// </summary>
        [HttpPost("process-payment")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] ProcessPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ProcessPaymentCommand(
                request.OrderNumber,
                request.PaymentMethod,
                request.PaymentReference,
                request.GatewayResponse
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse("Payment processed successfully."));
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpPost("{orderNumber}/cancel")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(
            string orderNumber,
            [FromBody] CancelOrderRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CancelOrderCommand(orderNumber, request.CancellationReason);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse("Order cancelled successfully."));
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", module = "Sales" });
        }

        private UserInfo GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            string userName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            return new UserInfo
            {
                Email = userEmail,
                CustomerId = Guid.Parse(userIdClaim),
                Name = userName,
            };
        }


        private class UserInfo
        {
            public string Email { get; set; }
            public Guid CustomerId { get; set; }
            public string Name { get; set; }
        }
    }
}
