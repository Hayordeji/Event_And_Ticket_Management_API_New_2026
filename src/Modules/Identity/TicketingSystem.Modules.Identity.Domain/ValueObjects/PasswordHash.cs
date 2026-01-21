using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.ValueObjects
{
    ///<summary>
/// Password hash value object
/// Stores BCrypt hashed password
/// </summary>
    public class PasswordHash : ValueObject
    {
        public string Value { get; private set; }

        private PasswordHash(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a password hash from plain text password
        /// </summary>
        public static PasswordHash CreateHash(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                throw new ArgumentException("Password cannot be empty", nameof(plainTextPassword));

            var hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword, workFactor: 12);
            return new PasswordHash(hash);
        }

        /// <summary>
        /// Create from existing hash (e.g., when loading from database)
        /// </summary>
        public static PasswordHash FromHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash cannot be empty", nameof(hash));

            return new PasswordHash(hash);
        }

        /// <summary>
        /// Verify if plain text password matches this hash
        /// </summary>
        public bool Verify(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, Value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
