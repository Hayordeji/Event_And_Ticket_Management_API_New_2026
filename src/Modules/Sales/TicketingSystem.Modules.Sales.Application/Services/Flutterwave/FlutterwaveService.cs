using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TicketingSystem.Modules.Sales.Application.DTOs;
using TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Application.Services.Flutterwave
{
    public class FlutterwaveService : IPaymentGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly FlutterwaveConfig _config;
        private readonly ILogger<FlutterwaveService> _logger;

        public string GatewayName => "Flutterwave";

        public FlutterwaveService(
            HttpClient httpClient,
            IOptions<FlutterwaveConfig> config,
            ILogger<FlutterwaveService> logger)
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
                // Generate unique tx_ref (idempotency key)
                var txRef = $"{orderNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                var request = new FlutterwaveInitializeRequest
                {
                    tx_ref = txRef,
                    amount = amount,
                    currency = currency,
                    redirect_url = callbackUrl,
                    customer = new FlutterwaveCustomer
                    {
                        email = customerEmail,
                        name = customerEmail.Split('@')[0]
                    },
                    meta = new Dictionary<string, string>
                    {
                        { "order_number", orderNumber }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "/payments",
                    request,
                    cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Flutterwave initialization failed: {Content}", content);
                    return Result.Failure<PaymentInitializationResponse>(
                        $"Payment initialization failed: {content}");
                }

                var flutterwaveResponse = JsonSerializer.Deserialize<FlutterwaveInitializeResponse>(content);

                if (flutterwaveResponse?.status != "success" || flutterwaveResponse.data == null)
                {
                    return Result.Failure<PaymentInitializationResponse>(
                        flutterwaveResponse?.message ?? "Payment initialization failed");
                }

                return Result.Success(new PaymentInitializationResponse(
                    PaymentReference: txRef,
                    AuthorizationUrl: flutterwaveResponse.data.link,
                    AccessCode: txRef
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Flutterwave payment for order {OrderNumber}", orderNumber);
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
                    $"/transactions/verify_by_reference?tx_ref={paymentReference}",
                    cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Flutterwave verification failed: {Content}", content);
                    return Result.Failure<PaymentVerificationResponse>(
                        $"Payment verification failed: {content}");
                }

                var flutterwaveResponse = JsonSerializer.Deserialize<FlutterwaveVerifyResponse>(content);

                if (flutterwaveResponse?.status != "success" || flutterwaveResponse.data == null)
                {
                    return Result.Failure<PaymentVerificationResponse>(
                        flutterwaveResponse?.message ?? "Payment verification failed");
                }

                var data = flutterwaveResponse.data;
                var isSuccessful = data.status.Equals("successful", StringComparison.OrdinalIgnoreCase);

                return Result.Success(new PaymentVerificationResponse(
                    IsSuccessful: isSuccessful,
                    PaymentReference: data.tx_ref,
                    Amount: data.amount,
                    Currency: data.currency,
                    CustomerEmail: data.customer?.email ?? string.Empty,
                    PaidAt: DateTime.TryParse(data.created_at, out var createdAt) ? createdAt : DateTime.UtcNow,
                    RawResponse: content
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Flutterwave payment {Reference}", paymentReference);
                return Result.Failure<PaymentVerificationResponse>(
                    $"Payment verification error: {ex.Message}");
            }
        }

        public bool VerifyWebhookSignature(string signature, string payload)
        {
            try
            {
                // Flutterwave sends the secret hash directly as verif-hash header
                return signature.Equals(_config.WebhookSecretHash, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Flutterwave webhook signature");
                return false;
            }
        }
    }
}
