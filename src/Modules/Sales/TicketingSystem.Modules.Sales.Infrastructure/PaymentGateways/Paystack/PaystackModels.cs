using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Infrastructure.PaymentGateways.Paystack
{
   
    //Initialize Payment Request
    public class PaystackInitializeRequest
    {
        public string email { get; set; } = null!;
        public long amount { get; set; }  // In kobo (multiply NGN by 100)
        public string reference { get; set; } = null!;
        public string callback_url { get; set; } = null!;
        public Dictionary<string, string>? metadata { get; set; }
    }

    // Initialize Payment Response
    public class PaystackInitializeResponse
    {
        public bool status { get; set; }
        public string message { get; set; } = null!;
        public PaystackInitializeData? data { get; set; }
    }

    public class PaystackInitializeData
    {
        public string authorization_url { get; set; } = null!;
        public string access_code { get; set; } = null!;
        public string reference { get; set; } = null!;
    }

    // Verify Payment Response
    public class PaystackVerifyResponse
    {
        public bool status { get; set; }
        public string message { get; set; } = null!;
        public PaystackVerifyData? data { get; set; }
    }

    public class PaystackVerifyData
    {
        public string status { get; set; } = null!;  // "success" or "failed"
        public string reference { get; set; } = null!;
        public long amount { get; set; }  // In kobo
        public string currency { get; set; } = null!;
        public PaystackCustomer? customer { get; set; }
        public string paid_at { get; set; } = null!;
    }

    public class PaystackCustomer
    {
        public string email { get; set; } = null!;
    }

    // Webhook Payload
    public class PaystackWebhookPayload
    {
        public string @event { get; set; } = null!;  // "charge.success"
        public PaystackWebhookData data { get; set; } = null!;
    }

    public class PaystackWebhookData
    {
        public long id { get; set; }
        public string reference { get; set; } = null!;
        public string status { get; set; } = null!;
        public long amount { get; set; }
        public string currency { get; set; } = null!;
        public PaystackCustomer? customer { get; set; }
        public string paid_at { get; set; } = null!;
    }
}
