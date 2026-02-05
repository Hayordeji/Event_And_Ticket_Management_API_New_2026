using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Flutterwave
{
    //Initialize Payment Request
    public class FlutterwaveInitializeRequest
    {
        public string tx_ref { get; set; } = null!;
        public decimal amount { get; set; }
        public string currency { get; set; } = null!;
        public string redirect_url { get; set; } = null!;
        public FlutterwaveCustomer customer { get; set; } = null!;
        public Dictionary<string, string>? meta { get; set; }
    }

    public class FlutterwaveCustomer
    {
        public string email { get; set; } = null!;
        public string name { get; set; } = null!;
    }

    // Initialize Payment Response
    public class FlutterwaveInitializeResponse
    {
        public string status { get; set; } = null!;
        public string message { get; set; } = null!;
        public FlutterwaveInitializeData? data { get; set; }
    }

    public class FlutterwaveInitializeData
    {
        public string link { get; set; } = null!;
    }

    // Verify Payment Response
    public class FlutterwaveVerifyResponse
    {
        public string status { get; set; } = null!;
        public string message { get; set; } = null!;
        public FlutterwaveVerifyData? data { get; set; }
    }

    public class FlutterwaveVerifyData
    {
        public string status { get; set; } = null!;  // "successful"
        public string tx_ref { get; set; } = null!;
        public decimal amount { get; set; }
        public string currency { get; set; } = null!;
        public FlutterwaveCustomer? customer { get; set; }
        public string created_at { get; set; } = null!;
    }

    // Webhook Payload
    public class FlutterwaveWebhookPayload
    {
        public string @event { get; set; } = null!;  // "charge.completed"
        public FlutterwaveWebhookData data { get; set; } = null!;
    }

    public class FlutterwaveWebhookData
    {
        public long id { get; set; }
        public string tx_ref { get; set; } = null!;
        public string status { get; set; } = null!;
        public decimal amount { get; set; }
        public string currency { get; set; } = null!;
        public FlutterwaveCustomer? customer { get; set; }
        public string created_at { get; set; } = null!;
    }
}
