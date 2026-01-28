using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Sales.Domain.ValueObjects
{
    /// <summary>
    /// Order number value object - unique, human-readable order identifier
    /// Format: ORD-YYYYMMDD-XXXX (e.g., ORD-20260125-A3F9)
    /// </summary>
    public class OrderNumber : ValueObject
    {
        public string Value { get; private set; }

        private OrderNumber(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Create OrderNumber from existing value (e.g., from database)
        /// </summary>
        public static Result<OrderNumber> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Failure<OrderNumber>("Order number cannot be empty");

            value = value.Trim().ToUpperInvariant();

            if (!IsValidFormat(value))
                return Result.Failure<OrderNumber>("Invalid order number format. Expected: ORD-YYYYMMDD-XXXX");

            return Result.Success(new OrderNumber(value));
        }

        /// <summary>
        /// Generate new unique order number
        /// Format: ORD-YYYYMMDD-XXXX where XXXX is random alphanumeric
        /// </summary>
        public static OrderNumber Generate()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = GenerateRandomCode(4);
            var value = $"ORD-{datePart}-{randomPart}";

            return new OrderNumber(value);
        }

        /// <summary>
        /// Validate order number format
        /// </summary>
        private static bool IsValidFormat(string value)
        {
            if (value.Length != 17) // ORD-20260125-A3F9 = 17 chars
                return false;

            if (!value.StartsWith("ORD-"))
                return false;

            var parts = value.Split('-');
            if (parts.Length != 3)
                return false;

            // Validate date part (8 digits)
            if (parts[1].Length != 8 || !parts[1].All(char.IsDigit))
                return false;

            // Validate random part (4 alphanumeric chars)
            if (parts[2].Length != 4 || !parts[2].All(char.IsLetterOrDigit))
                return false;

            return true;
        }

        /// <summary>
        /// Generate random alphanumeric code
        /// </summary>
        private static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude similar chars (0,O,1,I)
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // Implicit conversion to string for convenience
        public static implicit operator string(OrderNumber orderNumber) => orderNumber.Value;
    }
}
