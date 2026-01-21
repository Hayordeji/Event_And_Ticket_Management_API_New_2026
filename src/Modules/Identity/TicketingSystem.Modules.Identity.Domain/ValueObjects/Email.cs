using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.ValueObjects
{
    /// <summary>
/// Email value object with validation
/// </summary>
    public class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; private set; }

        private Email(string value)
        {
            Value = value;
        }

        public static Result<Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure<Email>("Email cannot be empty");

            email = email.Trim().ToLowerInvariant();

            if (email.Length > 255)
                return Result.Failure<Email>("Email cannot exceed 255 characters");

            if (!EmailRegex.IsMatch(email))
                return Result.Failure<Email>("Email format is invalid");

            return Result.Success(new Email(email));
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        // Implicit conversion to string for convenience
        public static implicit operator string(Email email) => email.Value;
    }
}
