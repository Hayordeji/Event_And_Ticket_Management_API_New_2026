using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Finance.Domain.Enums;
using TicketingSystem.Modules.Finance.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.Entities
{
    /// <summary>
    /// Ledger Account aggregate root
    /// Represents a financial account that holds a balance
    /// Examples: "Platform Revenue", "Host Pending Payouts", "Payment Gateway Settlement"
    /// </summary>
    public class LedgerAccount : AggregateRoot
    {
        public string AccountName { get; private set; } = string.Empty;
        public string AccountCode { get; private set; } = string.Empty;
        public AccountType AccountType { get; private set; }
        public Money CurrentBalance { get; private set; } = null!;
        public string Description { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        /// <summary>
        /// Concurrency token for optimistic locking
        /// Prevents lost updates when multiple transactions try to update balance simultaneously
        /// </summary>
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        // EF Core constructor
        private LedgerAccount() { }

        private LedgerAccount(string accountName, string accountCode, AccountType accountType, Money initialBalance, string description)
            : base()
        {
            AccountName = accountName;
            AccountCode = accountCode;
            AccountType = accountType;
            CurrentBalance = initialBalance;
            Description = description;
            IsActive = true;
        }

        /// <summary>
        /// Create a new ledger account
        /// </summary>
        public static Result<LedgerAccount> Create(
            string accountName,
            string accountCode,
            AccountType accountType,
            string currency = "NGN",
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(accountName))
                return Result.Failure<LedgerAccount>("Account name is required");

            if (accountName.Length > 100)
                return Result.Failure<LedgerAccount>("Account name cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(accountCode))
                return Result.Failure<LedgerAccount>("Account code is required");

            if (accountCode.Length > 20)
                return Result.Failure<LedgerAccount>("Account code cannot exceed 20 characters");

            var initialBalance = Money.Zero(currency);

            var account = new LedgerAccount(
                accountName.Trim(),
                accountCode.Trim().ToUpperInvariant(),
                accountType,
                initialBalance,
                description?.Trim() ?? string.Empty);

            return Result.Success(account);
        }

        /// <summary>
        /// Update account balance
        /// Called by LedgerTransaction when posting entries
        /// </summary>
        public void UpdateBalance(Money amount, EntryType entryType)
        {
            if (amount.Currency != CurrentBalance.Currency)
                throw new InvalidOperationException($"Currency mismatch: Account uses {CurrentBalance.Currency}, entry uses {amount.Currency}");

            // Determine if this entry increases or decreases the account balance
            // Based on accounting rules:
            // - Debits increase Assets and Expenses
            // - Credits increase Liabilities and Revenue
            bool increases = (AccountType == AccountType.Asset || AccountType == AccountType.Expense)
                ? entryType == EntryType.Debit
                : entryType == EntryType.Credit;

            CurrentBalance = increases
                ? CurrentBalance.Add(amount)
                : CurrentBalance.Subtract(amount);
        }

        /// <summary>
        /// Deactivate account (soft delete)
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Activate account
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }
    }
}
