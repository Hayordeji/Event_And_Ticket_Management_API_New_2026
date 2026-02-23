using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Application.Queries;
using TicketingSystem.SharedKernel.ApiResponses;
using TicketingSystem.SharedKernel.Authorization;

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
        [Authorize(Policy = PolicyNames.RequireCustomer)]
        [EnableRateLimiting("fixed_create_endpoints")]
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
                customer.Email,
                customer.Name,
                request.Items
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return CreatedAtAction(
                nameof(GetOrderByNumber),
                new { orderNumber = result.Value },
                ApiResponse<string>.SuccessResponse(result.Value, "Order created successfully.")
            );
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("{orderNumber}")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [Authorize(Policy = PolicyNames.RequireCustomer)]
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
        [EnableRateLimiting("fixed_get_endpoints")]
        [Authorize(Policy = PolicyNames.RequireCustomer)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
        {
            var customer = GetCurrentUserId();
            var query = new GetCustomerOrdersQuery(customer.CustomerId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(ApiResponse<IEnumerable<OrderResponse>>.SuccessResponse(result.Value));
        }

        ///// <summary>
        ///// Process payment for an order
        ///// </summary>
        //[HttpPost("process-payment")]
        //[Authorize(Policy = PolicyNames.RequireCustomer)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> ProcessPayment(
        //    [FromBody] ProcessPaymentRequest request,
        //    CancellationToken cancellationToken)
        //{
        //    var command = new ProcessPaymentCommand(
        //        request.OrderNumber,
        //        request.PaymentMethod,
        //        request.PaymentReference,
        //        request.GatewayResponse
        //    );

        //    var result = await _mediator.Send(command, cancellationToken);

        //    if (!result.IsSuccess)
        //        return BadRequest(ApiResponse.ErrorResponse(result.Error));

        //    return Ok(ApiResponse.SuccessResponse("Payment processed successfully."));
        //}


       
        /// <summary>
        /// Cancel an order
        /// </summary>
        [HttpPost("{orderNumber}/cancel")]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireCustomer)]
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
        /// Cancel an order
        /// </summary>
        [HttpPost("{orderNumber}/refund")]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireAdmin)] 
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestOrderRefund(
            string orderNumber,
            [FromBody] RefundOrderRequest request,
            CancellationToken cancellationToken)
        {
            var command = new RequestRefundCommand(orderNumber, request.RefundReason);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse("Order refunded successfully."));
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [EnableRateLimiting("fixed_get_endpoints")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", module = "Sales" });
        }

        private UserInfo GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            string userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            string userName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            return new UserInfo
            {
                Email = userEmail,
                CustomerId = Guid.Parse(userIdClaim),
                Name = userName,
            };
        }

        ///<summary>
        /// Initialize payment for an order
        /// </summary>
        [HttpPost("{orderNumber}/initialize-payment")]
        [EnableRateLimiting("fixed_create_endpoints")]
        [Authorize(Policy = PolicyNames.RequireCustomer)]
        [ProducesResponseType(typeof(ApiResponse<PaymentInitializationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitializePayment(
            string orderNumber,
            [FromBody] InitializePaymentRequest request,
            CancellationToken cancellationToken)
        {
            var customerId = GetCurrentUserId();

            var customerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "customer@example.com";

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/webhooks/verify-payment";

            var command = new InitializePaymentCommand(
                orderNumber,
                request.Gateway,
                customerEmail,
                callbackUrl
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse<PaymentInitializationResponse>.SuccessResponse(
                result.Value,
                "Payment initialized successfully. Redirect user to authorization URL."));
        }

        private class UserInfo
        {
            public string Email { get; set; }
            public Guid CustomerId { get; set; }
            public string Name { get; set; }
        }
    }
}
