using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Domain.Events;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.Entities
{
     ///<summary>
/// Ledger Transaction aggregate root
/// Represents an atomic financial event (e.g., "Ticket Purchase", "Host Payout")
/// Must always balance to zero (Sum of Debits = Sum of Credits)
/// </summary>
    public class LedgerTransaction : AggregateRoot
    {
        public string ReferenceType { get; private set; } = string.Empty;
        public Guid ReferenceId { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public DateTime OccurredAt { get; private set; }
        public bool IsPosted { get; private set; }
        public DateTime? PostedAt { get; private set; }

        private readonly List<LedgerEntry> _entries = new();
        public IReadOnlyCollection<LedgerEntry> Entries => _entries.AsReadOnly();

        // EF Core constructor
        private LedgerTransaction() { }

        private LedgerTransaction(string referenceType, Guid referenceId, string description, DateTime occurredAt)
            : base()
        {
            ReferenceType = referenceType;
            ReferenceId = referenceId;
            Description = description;
            OccurredAt = occurredAt;
            IsPosted = false;
        }

        /// <summary>
        /// Create a new ledger transaction
        /// </summary>
        public static Result<LedgerTransaction> Create(
            string referenceType,
            Guid referenceId,
            string description,
            DateTime? occurredAt = null)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
                return Result.Failure<LedgerTransaction>("Reference type is required");

            if (referenceType.Length > 50)
                return Result.Failure<LedgerTransaction>("Reference type cannot exceed 50 characters");

            if (referenceId == Guid.Empty)
                return Result.Failure<LedgerTransaction>("Reference ID cannot be empty");

            if (string.IsNullOrWhiteSpace(description))
                return Result.Failure<LedgerTransaction>("Description is required");

            if (description.Length > 500)
                return Result.Failure<LedgerTransaction>("Description cannot exceed 500 characters");

            var transaction = new LedgerTransaction(
                referenceType.Trim(),
                referenceId,
                description.Trim(),
                occurredAt ?? DateTime.UtcNow);

            return Result.Success(transaction);
        }

        /// <summary>
        /// Add a debit entry to the transaction
        /// </summary>
        public Result AddDebit(Guid accountId, Money amount, string? description = null)
        {
            return AddEntry(accountId, amount, EntryType.Debit, description);
        }

        /// <summary>
        /// Add a credit entry to the transaction
        /// </summary>
        public Result AddCredit(Guid accountId, Money amount, string? description = null)
        {
            return AddEntry(accountId, amount, EntryType.Credit, description);
        }

        private Result AddEntry(Guid accountId, Money amount, EntryType entryType, string? description)
        {
            if (IsPosted)
                return Result.Failure("Cannot add entries to a posted transaction");

            if (accountId == Guid.Empty)
                return Result.Failure("Account ID cannot be empty");

            var entry = new LedgerEntry(
                Id,
                accountId,
                amount,
                entryType,
                description ?? Description);

            _entries.Add(entry);
            return Result.Success();
        }

        /// <summary>
        /// Validate that the transaction balances (Sum of Debits = Sum of Credits)
        /// </summary>
        public Result Validate()
        {
            if (_entries.Count < 2)
                return Result.Failure("Transaction must have at least 2 entries (one debit and one credit)");

            var debits = _entries.Where(e => e.EntryType == EntryType.Debit).ToList();
            var credits = _entries.Where(e => e.EntryType == EntryType.Credit).ToList();

            if (!debits.Any())
                return Result.Failure("Transaction must have at least one debit entry");

            if (!credits.Any())
                return Result.Failure("Transaction must have at least one credit entry");

            // Check currency consistency
            var currencies = _entries.Select(e => e.Amount.Currency).Distinct().ToList();
            if (currencies.Count > 1)
                return Result.Failure($"All entries must use the same currency. Found: {string.Join(", ", currencies)}");

            // Calculate totals
            var totalDebits = debits.Sum(e => e.Amount.Amount);
            var totalCredits = credits.Sum(e => e.Amount.Amount);

            if (totalDebits != totalCredits)
                return Result.Failure($"Transaction does not balance. Debits: {totalDebits:N2}, Credits: {totalCredits:N2}");

            return Result.Success();
        }

        /// <summary>
        /// Post the transaction (make it permanent)
        /// This updates all account balances
        /// </summary>
        public Result Post()
        {
            if (IsPosted)
                return Result.Failure("Transaction is already posted");

            var validationResult = Validate();
            if (validationResult.IsFailure)
                return validationResult;

            IsPosted = true;
            PostedAt = DateTime.UtcNow;

            // Raise domain event
            var totalAmount = _entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount.Amount);
            var currency = _entries.First().Amount.Currency;

            RaiseDomainEvent(new TransactionRecordedEvent(
                Id,
                ReferenceType,
                ReferenceId,
                totalAmount,
                currency,
                OccurredAt));

            return Result.Success();
        }
    }
}
