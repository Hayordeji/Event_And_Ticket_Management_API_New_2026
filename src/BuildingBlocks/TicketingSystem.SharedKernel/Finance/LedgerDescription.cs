using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Finance
{
    /// <summary>
    /// Standardized description templates for ledger entries.
    /// Ensures consistent audit trail wording across all financial transactions.
    /// </summary>
    public static class LedgerDescriptions
    {
        // ── Order Payment ─────────────────────────────────────────────────────────
        public static string PaymentReceived(string orderNumber)
            => $"Payment received for order {orderNumber}";

        public static string PlatformCommissionEarned(string orderNumber)
            => $"Platform commission earned — {orderNumber}";

        public static string HostEarningsRecorded(string orderNumber)
            => $"Host earnings recorded — {orderNumber}";

        // ── Order Cancellation ────────────────────────────────────────────────────
        public static string HostEarningsReversal(string orderNumber)
            => $"Host earnings reversal — {orderNumber}";

        public static string PlatformCommissionReversal(string orderNumber)
            => $"Platform commission reversal — {orderNumber}";

        public static string GatewayCreditForCancellation(string orderNumber)
            => $"Gateway credit for cancellation — {orderNumber}";

        // ── Order Refund ──────────────────────────────────────────────────────────
        public static string HostEarningsRefunded(string orderNumber)
            => $"Host earnings refunded — {orderNumber}";

        public static string PlatformCommissionAbsorbed(string orderNumber)
            => $"Platform commission absorbed as expense — {orderNumber}";

        public static string FullRefundToGateway(string orderNumber)
            => $"Full refund returned to gateway — {orderNumber}";

        // ── Host Payout ───────────────────────────────────────────────────────────
        public static string HostPayoutProcessed(string hostId, decimal amount, string currency)
            => $"Payout to host {hostId}: {amount} {currency}";

        public static string HostPayoutCompleted(string hostId)
            => $"Payout completed for host {hostId}";

        // ── Event Published ───────────────────────────────────────────────────────
        public static string HostAccountPreCreated(string hostId, string eventName)
            => $"Host account pre-created for event '{eventName}' (Host: {hostId})";

    }
}
