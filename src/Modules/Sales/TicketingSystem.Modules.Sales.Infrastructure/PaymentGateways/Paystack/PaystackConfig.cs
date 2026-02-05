using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack
{
    public class PaystackConfig
    {
        public string PublicKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string WebhookSecret { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://api.paystack.co";
    }
}
