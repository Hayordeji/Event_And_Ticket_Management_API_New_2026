using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Finance.Domain.ValueObjects
{
     ///<summary>
/// Money value object with currency
/// Ensures amounts are always positive and properly rounded
/// </summary>
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Result<Money> Create(decimal amount, string currency = "NGN")
        {
            if (amount < 0)
                return Result.Failure<Money>("Amount cannot be negative");

            if (string.IsNullOrWhiteSpace(currency))
                return Result.Failure<Money>("Currency is required");

            if (currency.Length != 3)
                return Result.Failure<Money>("Currency must be a 3-letter ISO code (e.g., NGN, USD)");

            // Round to 2 decimal places for currency
            var roundedAmount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            return Result.Success(new Money(roundedAmount, currency.ToUpperInvariant()));
        }

        public static Money Zero(string currency = "NGN") => new(0m, currency);

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");

            var result = Amount - other.Amount;
            if (result < 0)
                throw new InvalidOperationException("Result cannot be negative");

            return new Money(result, Currency);
        }

        public Money Multiply(decimal multiplier)
        {
            if (multiplier < 0)
                throw new InvalidOperationException("Multiplier cannot be negative");

            return new Money(Math.Round(Amount * multiplier, 2), Currency);
        }

        public static bool operator >(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Cannot compare different currencies");

            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Cannot compare different currencies");

            return left.Amount < right.Amount;
        }

        public static bool operator >=(Money left, Money right) => !(left < right);
        public static bool operator <=(Money left, Money right) => !(left > right);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString() => $"{Currency} {Amount:N2}";
    }
}
