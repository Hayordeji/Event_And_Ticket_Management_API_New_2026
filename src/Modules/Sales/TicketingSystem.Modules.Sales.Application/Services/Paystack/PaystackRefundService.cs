using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services.Paystack
{
    public class PaystackRefundService : IPaymentGatewayRefundService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackConfig _config;
        private readonly ILogger<PaystackRefundService> _logger;

        public string GatewayName => "Paystack";

        public PaystackRefundService(
            HttpClient httpClient,
            IOptions<PaystackConfig> config,
            ILogger<PaystackRefundService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.paystack.co");
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
                var amountInKobo = (int)(amount * 100); // Convert NGN to kobo

                var request = new PaystackRefundRequest
                {
                    Transaction = paymentReference,
                    Amount = amountInKobo,
                    Currency = currency,
                    CustomerNote = reason,
                    MerchantNote = "Testing the refund"
                };

                var response = await _httpClient.PostAsJsonAsync("/refund", request, ct);
                //var resultSting = await response.Content.ReadAsStringAsync();
                //var result = JsonSerializer.Deserialize<PaystackRefundApiResponse>(resultSting);

                var result = await response.Content.ReadFromJsonAsync<PaystackRefundApiResponse>(ct);

                if (result?.Status != true || result.Data == null)
                {
                    var error = result?.Message ?? "Unknown error from Paystack";
                    _logger.LogError("Paystack refund failed: {Error}", error);
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
                _logger.LogError(ex, "HTTP error calling Paystack refund API");
                return Result.Failure<RefundResponse>(
                    "Failed to communicate with payment gateway. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error initiating Paystack refund");
                return Result.Failure<RefundResponse>("An unexpected error occurred during refund.");
            }
        }

        // DTOs for Paystack API
        private record PaystackRefundRequest
        {
            [JsonPropertyName("transaction")]
            public required string Transaction { get; init; }

            [JsonPropertyName("amount")]
            public int Amount { get; init; }

            [JsonPropertyName("currency")]
            public required string Currency { get; init; }

            [JsonPropertyName("customer_note")]
            public required string CustomerNote { get; init; }

            [JsonPropertyName("merchant_note")]
            public required string MerchantNote { get; init; }
        }

        private record PaystackRefundApiResponse
        {
            [JsonPropertyName("status")]
            public bool Status { get; init; }

            [JsonPropertyName("message")]
            public string? Message { get; init; }

            [JsonPropertyName("data")]
            public PaystackRefundData? Data { get; init; }
        }

        private record PaystackRefundData
        {
            [JsonPropertyName("id")]
            public int Id { get; init; }

            [JsonPropertyName("status")]
            public required string Status { get; init; }

            [JsonPropertyName("transaction_reference")]
            public required string TransactionReference { get; init; }
        }
    }
}
