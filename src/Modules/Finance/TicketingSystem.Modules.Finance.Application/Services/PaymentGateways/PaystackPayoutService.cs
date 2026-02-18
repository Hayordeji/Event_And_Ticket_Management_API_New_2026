using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Application.Services.PaymentGateways
{
    public class PaystackPayoutService : IPayoutService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackConfig _config;
        private readonly ILogger<PaystackPayoutService> _logger;

        public PaystackPayoutService(
            HttpClient httpClient,
            IOptions<PaystackConfig> config,
            ILogger<PaystackPayoutService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri("https://api.paystack.co");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.SecretKey}");
        }

        public async Task<Result<PayoutResponse>> InitiatePayoutAsync(
            Guid hostId,
            decimal amount,
            string currency,
            string bankAccountNumber,
            string bankCode,
            string reason,
            CancellationToken ct = default)
        {
            try
            {
                // Step 1: Create transfer recipient (should be cached in production)
                var recipientResult = await CreateRecipientAsync(
                    bankAccountNumber,
                    bankCode,
                    $"Host-{hostId}",
                    ct);

                if (recipientResult.IsFailure)
                    return Result.Failure<PayoutResponse>(recipientResult.Error);

                // Step 2: Initiate transfer
                var amountInKobo = (int)(amount * 100);

                var transferRequest = new PaystackTransferRequest
                {
                    Source = "balance",
                    Reason = reason,
                    Amount = amountInKobo,
                    Recipient = recipientResult.Value,
                    Currency = currency
                };

                var response = await _httpClient.PostAsJsonAsync("/transfer", transferRequest, ct);
                var result = await response.Content.ReadFromJsonAsync<PaystackTransferApiResponse>(ct);

                if (result?.Status != true || result.Data == null)
                {
                    var error = result?.Message ?? "Unknown error from Paystack";
                    _logger.LogError("Paystack transfer failed: {Error}", error);
                    return Result.Failure<PayoutResponse>(error);
                }

                return Result.Success(new PayoutResponse(
                    PayoutReference: result.Data.Reference,
                    Status: result.Data.Status,
                    Message: result.Message ?? "Transfer initiated",
                    ProcessedAt: DateTime.UtcNow
                ));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Paystack transfer API");
                return Result.Failure<PayoutResponse>(
                    "Failed to communicate with payment gateway.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error initiating Paystack transfer");
                return Result.Failure<PayoutResponse>("An unexpected error occurred.");
            }
        }

        private async Task<Result<string>> CreateRecipientAsync(
            string accountNumber,
            string bankCode,
            string name,
            CancellationToken ct)
        {
            try
            {
                var request = new PaystackRecipientRequest
                {
                    Type = "nuban",
                    Name = name,
                    AccountNumber = accountNumber,
                    BankCode = bankCode,
                    Currency = "NGN"
                };

                var response = await _httpClient.PostAsJsonAsync("/transferrecipient", request, ct);
                var result = await response.Content.ReadFromJsonAsync<PaystackRecipientApiResponse>(ct);

                if (result?.Status != true || result.Data == null)
                {
                    var error = result?.Message ?? "Failed to create transfer recipient";
                    return Result.Failure<string>(error);
                }

                return Result.Success(result.Data.RecipientCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Paystack transfer recipient");
                return Result.Failure<string>("Failed to create transfer recipient.");
            }
        }

        // DTOs
        private record PaystackRecipientRequest
        {
            [JsonPropertyName("type")]
            public required string Type { get; init; }

            [JsonPropertyName("name")]
            public required string Name { get; init; }

            [JsonPropertyName("account_number")]
            public required string AccountNumber { get; init; }

            [JsonPropertyName("bank_code")]
            public required string BankCode { get; init; }

            [JsonPropertyName("currency")]
            public required string Currency { get; init; }
        }

        private record PaystackRecipientApiResponse
        {
            [JsonPropertyName("status")]
            public bool Status { get; init; }

            [JsonPropertyName("message")]
            public string? Message { get; init; }

            [JsonPropertyName("data")]
            public PaystackRecipientData? Data { get; init; }
        }

        private record PaystackRecipientData
        {
            [JsonPropertyName("recipient_code")]
            public required string RecipientCode { get; init; }
        }

        private record PaystackTransferRequest
        {
            [JsonPropertyName("source")]
            public required string Source { get; init; }

            [JsonPropertyName("reason")]
            public required string Reason { get; init; }

            [JsonPropertyName("amount")]
            public int Amount { get; init; }

            [JsonPropertyName("recipient")]
            public required string Recipient { get; init; }

            [JsonPropertyName("currency")]
            public required string Currency { get; init; }
        }

        private record PaystackTransferApiResponse
        {
            [JsonPropertyName("status")]
            public bool Status { get; init; }

            [JsonPropertyName("message")]
            public string? Message { get; init; }

            [JsonPropertyName("data")]
            public PaystackTransferData? Data { get; init; }
        }

        private record PaystackTransferData
        {
            [JsonPropertyName("reference")]
            public required string Reference { get; init; }

            [JsonPropertyName("status")]
            public required string Status { get; init; }

            [JsonPropertyName("transfer_code")]
            public required string TransferCode { get; init; }
        }
    }
}
