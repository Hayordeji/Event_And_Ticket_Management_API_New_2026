using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.Entities
{
    /// <summary>
    /// Ledger Entry entity
    /// Represents a single line in a double-entry transaction
    /// Each transaction must have at least 2 entries (one debit, one credit)
    /// </summary>
    public class LedgerEntry : Entity
    {
        public Guid TransactionId { get; private set; }
        public Guid AccountId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public EntryType EntryType { get; private set; }
        public string Description { get; private set; } = string.Empty;

        // Navigation properties
        public LedgerAccount Account { get; private set; } = null!;

        // EF Core constructor
        private LedgerEntry() { }

        internal LedgerEntry(
            Guid transactionId,
            Guid accountId,
            Money amount,
            EntryType entryType,
            string description)
            : base()
        {
            TransactionId = transactionId;
            AccountId = accountId;
            Amount = amount;
            EntryType = entryType;
            Description = description;
        }
    }
}
