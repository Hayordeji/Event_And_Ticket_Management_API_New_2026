using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Application.DTOs
{
    public record InitializePaymentRequest(
    string Gateway  // "Paystack" or "Flutterwave"
    );
}
