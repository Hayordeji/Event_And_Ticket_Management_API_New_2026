using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Domain.Enums
{
    public enum OrderStatus
    {
        /// <summary>
        /// Order created, awaiting payment
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Payment received and confirmed
        /// </summary>
        Paid = 1,

        /// <summary>
        /// Tickets generated and delivered to customer
        /// </summary>
        Fulfilled = 2,

        /// <summary>
        /// Order cancelled before payment (tickets released)
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Payment refunded (tickets invalidated)
        /// </summary>
        Refunded = 4,

        /// <summary>
        /// Order expired (payment not received within time limit)
        /// </summary>
        Expired = 5
    }

    /// <summary>
    /// Payment transaction states
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Payment initialized, awaiting gateway response
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Payment being processed by gateway
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Payment successful and confirmed
        /// </summary>
        Successful = 2,

        /// <summary>
        /// Payment failed or declined
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Payment refunded to customer
        /// </summary>
        Refunded = 4,

        /// <summary>
        /// Payment abandoned by customer
        /// </summary>
        Abandoned = 5
    }

    /// <summary>
    /// Supported payment gateway providers
    /// </summary>
    public enum PaymentGateway
    {
        /// <summary>
        /// Paystack payment gateway
        /// </summary>
        Paystack = 0,

        /// <summary>
        /// Flutterwave payment gateway
        /// </summary>
        Flutterwave = 1,

        /// <summary>
        /// Manual payment (bank transfer, cash)
        /// </summary>
        Manual = 2
    }
}
