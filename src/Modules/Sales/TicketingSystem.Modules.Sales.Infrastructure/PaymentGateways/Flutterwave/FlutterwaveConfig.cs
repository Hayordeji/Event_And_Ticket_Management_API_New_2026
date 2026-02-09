using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave
{
    public class FlutterwaveConfig
    {
        public string PublicKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string EncryptionKey { get; set; } = null!;
        public string WebhookSecretHash { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://api.flutterwave.com/v3";
    }
}
