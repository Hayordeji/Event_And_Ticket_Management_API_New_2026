using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services.Paystack
{
    public class PaystackService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackConfig _config;
        private readonly ILogger<PaystackService> _logger;

        public string GatewayName => "Paystack";

        public PaystackService(
            HttpClient httpClient,
            IOptions<PaystackConfig> config,
            ILogger<PaystackService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.SecretKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Result<PaymentInitializationResponse>> InitializePaymentAsync(
            string orderNumber,
            decimal amount,
            string currency,
            string customerEmail,
            string callbackUrl,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Paystack expects amount in kobo (1 NGN = 100 kobo)
                var amountInKobo = (long)(amount * 100);

                // Generate unique reference (idempotency key)
                var reference = $"{orderNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                var request = new PaystackInitializeRequest
                {
                    email = customerEmail,
                    amount = amountInKobo,
                    reference = reference,
                    callback_url = callbackUrl,
                    metadata = new Dictionary<string, string>
                    {
                        { "order_number", orderNumber }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/transaction/initialize",
                    request,
                    cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paystack initialization failed: {Content}", content);
                    return Result.Failure<PaymentInitializationResponse>(
                        $"Payment initialization failed: {content}");
                }

                var paystackResponse = JsonSerializer.Deserialize<PaystackInitializeResponse>(content);

                if (paystackResponse?.status != true || paystackResponse.data == null)
                {
                    return Result.Failure<PaymentInitializationResponse>(
                        paystackResponse?.message ?? "Payment initialization failed");
                }

                return Result.Success(new PaymentInitializationResponse(
                    PaymentReference: paystackResponse.data.reference,
                    AuthorizationUrl: paystackResponse.data.authorization_url,
                    AccessCode: paystackResponse.data.access_code
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paystack payment for order {OrderNumber}", orderNumber);
                return Result.Failure<PaymentInitializationResponse>(
                    $"Payment initialization error: {ex.Message}");
            }
        }

        public async Task<Result<PaymentVerificationResponse>> VerifyPaymentAsync(
            string paymentReference,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"/transaction/verify/{paymentReference}",
                    cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Paystack verification failed: {Content}", content);
                    return Result.Failure<PaymentVerificationResponse>(
                        $"Payment verification failed: {content}");
                }

                var paystackResponse = JsonSerializer.Deserialize<PaystackVerifyResponse>(content);

                if (paystackResponse?.status != true || paystackResponse.data == null)
                {
                    return Result.Failure<PaymentVerificationResponse>(
                        paystackResponse?.message ?? "Payment verification failed");
                }

                var data = paystackResponse.data;
                var isSuccessful = data.status.Equals("success", StringComparison.OrdinalIgnoreCase);

                return Result.Success(new PaymentVerificationResponse(
                    IsSuccessful: isSuccessful,
                    PaymentReference: data.reference,
                    Amount: data.amount / 100m,  // Convert from kobo to NGN
                    Currency: data.currency,
                    CustomerEmail: data.customer?.email ?? string.Empty,
                    PaidAt: DateTime.TryParse(data.paid_at, out var paidAt) ? paidAt : DateTime.UtcNow,
                    RawResponse: content
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Paystack payment {Reference}", paymentReference);
                return Result.Failure<PaymentVerificationResponse>(
                    $"Payment verification error: {ex.Message}");
            }
        }

        public bool VerifyWebhookSignature(string signature, string payload)
        {
            try
            {
                var hash = ComputeHmacSha512(_config.WebhookSecret, payload);
                return hash.Equals(signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Paystack webhook signature");
                return false;
            }
        }

        private static string ComputeHmacSha512(string secret, string payload)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
