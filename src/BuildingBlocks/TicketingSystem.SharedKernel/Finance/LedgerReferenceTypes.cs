using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Finance
{
    /// <summary>
    /// Standard reference types for ledger transactions.
    /// These are queried and filtered in reports, so consistency is critical.
    /// </summary>
    public static class LedgerReferenceTypes
    {
        public const string OrderPayment = "ORDER-PAYMENT";
        public const string OrderCancellation = "ORDER-CANCELLATION";
        public const string OrderRefund = "ORDER-REFUND";
        public const string HostPayout = "HOST-PAYOUT";
    }
}
