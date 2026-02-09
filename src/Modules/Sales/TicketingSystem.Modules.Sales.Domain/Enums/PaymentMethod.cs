using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Sales.Domain.Enums
{
    public enum PaymentMethod
    {
        Card = 0,
        BankTransfer = 1,
        USSD = 2,
        Wallet = 3,
        Cash = 4 // For on-site purchases
    }
}
