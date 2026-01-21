using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Finance.Domain.Enums
{
    ///<summary>
/// Type of ledger entry (Debit or Credit)
/// </summary>
    public enum EntryType
    {
        /// <summary>
        /// Debit entry - increases Assets and Expenses, decreases Liabilities and Revenue
        /// </summary>
        Debit = 1,

        /// <summary>
        /// Credit entry - increases Liabilities and Revenue, decreases Assets and Expenses
        /// </summary>
        Credit = 2
    }
}
