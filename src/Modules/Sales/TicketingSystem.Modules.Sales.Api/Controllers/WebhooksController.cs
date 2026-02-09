using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Sales.Application.Commands;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Application.Services;
using TicketingSystem.Modules.Sales.Domain.Entities;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack;
using TicketingSystem.Modules.Sales.Infrastructure.Persistence;
using TicketingSystem.SharedKernel.ApiResponses;

namespace TicketingSystem.Modules.Sales.Api.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SalesDbContext _context;
        private readonly IEnumerable<IPaymentGatewayService> _gatewayServices;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(
            IMediator mediator,
            SalesDbContext context,
            IEnumerable<IPaymentGatewayService> gatewayServices,
            ILogger<WebhooksController> logger)
        {
            _mediator = mediator;
            _context = context;
            _gatewayServices = gatewayServices;
            _logger = logger;
        }



        /// <summary>
        /// Process payment for an order
        /// </summary>
        [HttpGet("verify-payment")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyPayment(
            [FromQuery] VerifyPaymentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new VerifyPaymentCommand(
                request.trxref,
                "Paystack"
            );

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.ErrorResponse(result.Error));

            return Ok(ApiResponse.SuccessResponse("Payment processed successfully."));
        }


        /// <summary>
        /// Paystack webhook endpoint
        /// </summary>
        [HttpPost("paystack")]
        public async Task<IActionResult> PaystackWebhook([FromBody] PaystackWebhookPayload payload)
        {
            try
            {
                // Get request body for signature verification
                //Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();

                // Verify webhook signature
                var signature = Request.Headers["x-paystack-signature"].ToString();
                var paystackService = _gatewayServices
                    .FirstOrDefault(g => g.GatewayName == "Paystack");

                if (paystackService == null || !paystackService.VerifyWebhookSignature(signature, requestBody))
                {
                    _logger.LogWarning("Invalid Paystack webhook signature");
                    return Unauthorized();
                }

                // IDEMPOTENCY: Check if webhook event already processed
                var gatewayEventId = payload.data.id.ToString();
                var eventExists = await _context.Set<WebhookEvent>()
                    .AnyAsync(e => e.Gateway == "Paystack" && e.GatewayEventId == gatewayEventId);

                if (eventExists)
                {
                    _logger.LogInformation("Paystack webhook event {EventId} already processed", gatewayEventId);
                    return Ok(); // Already processed (idempotent)
                }

                // Record webhook event BEFORE processing (idempotency safeguard)
                var webhookEvent = WebhookEvent.Create(
                    gatewayEventId: gatewayEventId,
                    gateway: "Paystack",
                    eventType: payload.@event,
                    paymentReference: payload.data.reference,
                    rawPayload: requestBody
                );

                await _context.Set<WebhookEvent>().AddAsync(webhookEvent);
                await _context.SaveChangesAsync();

                // Process payment verification only for successful charges
                if (payload.@event.Equals("charge.success", StringComparison.OrdinalIgnoreCase))
                {
                    var command = new VerifyPaymentCommand(
                        PaymentReference: payload.data.reference,
                        Gateway: "Paystack"
                    );

                    var result = await _mediator.Send(command);

                    if (result.IsSuccess)
                    {
                        webhookEvent.MarkAsProcessed();
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paystack webhook");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Flutterwave webhook endpoint
        /// </summary>
        [HttpPost("flutterwave")]
        public async Task<IActionResult> FlutterwaveWebhook([FromBody] FlutterwaveWebhookPayload payload)
        {
            try
            {
                // Get request body for signature verification
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();

                // Verify webhook signature
                var signature = Request.Headers["verif-hash"].ToString();
                var flutterwaveService = _gatewayServices
                    .FirstOrDefault(g => g.GatewayName == "Flutterwave");

                if (flutterwaveService == null || !flutterwaveService.VerifyWebhookSignature(signature, requestBody))
                {
                    _logger.LogWarning("Invalid Flutterwave webhook signature");
                    return Unauthorized();
                }

                // IDEMPOTENCY: Check if webhook event already processed
                var gatewayEventId = payload.data.id.ToString();
                var eventExists = await _context.Set<WebhookEvent>()
                    .AnyAsync(e => e.Gateway == "Flutterwave" && e.GatewayEventId == gatewayEventId);

                if (eventExists)
                {
                    _logger.LogInformation("Flutterwave webhook event {EventId} already processed", gatewayEventId);
                    return Ok(); // Already processed (idempotent)
                }

                // Record webhook event BEFORE processing (idempotency safeguard)
                var webhookEvent = WebhookEvent.Create(
                    gatewayEventId: gatewayEventId,
                    gateway: "Flutterwave",
                    eventType: payload.@event,
                    paymentReference: payload.data.tx_ref,
                    rawPayload: requestBody
                );

                await _context.Set<WebhookEvent>().AddAsync(webhookEvent);
                await _context.SaveChangesAsync();

                // Process payment verification only for successful charges
                if (payload.@event.Equals("charge.completed", StringComparison.OrdinalIgnoreCase))
                {
                    var command = new VerifyPaymentCommand(
                        PaymentReference: payload.data.tx_ref,
                        Gateway: "Flutterwave"
                    );

                    var result = await _mediator.Send(command);

                    if (result.IsSuccess)
                    {
                        webhookEvent.MarkAsProcessed();
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Flutterwave webhook");
                return StatusCode(500);
            }
        }
    }
}
