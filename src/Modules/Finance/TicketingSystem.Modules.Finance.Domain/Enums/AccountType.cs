using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Finance.Domain.Enums
{
    ///<summary>
/// Types of ledger accounts based on accounting equation
/// Assets = Liabilities + Equity + (Revenue - Expenses)
/// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Assets - What we own or are owed
        /// Examples: Cash, Accounts Receivable
        /// Normal balance: Debit
        /// </summary>
        Asset = 1,

        /// <summary>
        /// Liabilities - What we owe to others
        /// Examples: Accounts Payable, Host Pending Payouts
        /// Normal balance: Credit
        /// </summary>
        Liability = 2,

        /// <summary>
        /// Revenue - Income earned
        /// Examples: Platform Commission, Service Fees
        /// Normal balance: Credit
        /// </summary>
        Revenue = 3,

        /// <summary>
        /// Expenses - Costs incurred
        /// Examples: Payment Processing Fees, Operational Costs
        /// Normal balance: Debit
        /// </summary>
        Expense = 4
    }
}
