using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services.Flutterwave
{
    public class FlutterwaveRefundService : IPaymentGatewayRefundService
    {
        private readonly HttpClient _httpClient;
        private readonly FlutterwaveConfig _config;
        private readonly ILogger<FlutterwaveRefundService> _logger;

        public string GatewayName => "Flutterwave";

        public FlutterwaveRefundService(
            HttpClient httpClient,
            IOptions<FlutterwaveConfig> config,
            ILogger<FlutterwaveRefundService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.flutterwave.com");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.SecretKey}");
        }

        public async Task<Result<RefundResponse>> InitiateRefundAsync(
            string paymentReference,
            decimal amount,
            string currency,
            string reason,
            CancellationToken ct = default)
        {
            try
            {
                // Note: Flutterwave needs transaction ID, not payment reference
                // In production, you'd need to store both or look up the transaction ID
                var request = new FlutterwaveRefundRequest
                {
                    Amount = amount,
                    Comments = reason
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"/v3/transactions/{paymentReference}/refund",
                    request,
                    ct);

                var result = await response.Content.ReadFromJsonAsync<FlutterwaveRefundApiResponse>(ct);

                if (result?.Status != "success" || result.Data == null)
                {
                    var error = result?.Message ?? "Unknown error from Flutterwave";
                    _logger.LogError("Flutterwave refund failed: {Error}", error);
                    return Result.Failure<RefundResponse>(error);
                }

                return Result.Success(new RefundResponse(
                    RefundReference: result.Data.Id.ToString(),
                    Status: result.Data.Status,
                    Message: result.Message ?? "Refund initiated",
                    ProcessedAt: DateTime.UtcNow
                ));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Flutterwave refund API");
                return Result.Failure<RefundResponse>(
                    "Failed to communicate with payment gateway. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error initiating Flutterwave refund");
                return Result.Failure<RefundResponse>("An unexpected error occurred during refund.");
            }
        }

        // DTOs for Flutterwave API
        private record FlutterwaveRefundRequest
        {
            [JsonPropertyName("amount")]
            public decimal Amount { get; init; }

            [JsonPropertyName("comments")]
            public required string Comments { get; init; }
        }

        private record FlutterwaveRefundApiResponse
        {
            [JsonPropertyName("status")]
            public required string Status { get; init; }

            [JsonPropertyName("message")]
            public string? Message { get; init; }

            [JsonPropertyName("data")]
            public FlutterwaveRefundData? Data { get; init; }
        }

        private record FlutterwaveRefundData
        {
            [JsonPropertyName("id")]
            public int Id { get; init; }

            [JsonPropertyName("status")]
            public required string Status { get; init; }

            [JsonPropertyName("tx_ref")]
            public required string TxRef { get; init; }
        }
    }
}
