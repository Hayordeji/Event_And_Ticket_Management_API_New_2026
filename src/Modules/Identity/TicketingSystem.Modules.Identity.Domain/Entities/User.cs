using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Events;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Entities
{
    public class User: AggregateRoot
    {
        public Email Email { get; private set; } = null!;
        public PasswordHash PasswordHash { get; private set; } = null!;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public DateTime? EmailVerifiedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation property for refresh tokens
        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        // EF Core constructor
        private User() { }

        private User(Email email, PasswordHash passwordHash, string firstName, string lastName, string? phoneNumber)
            : base()
        {
            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            IsEmailVerified = false;
            IsActive = true;

            // Raise domain event
            RaiseDomainEvent(new UserRegisteredEvent(Id, email.Value, DateTime.UtcNow));
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public static Result<User> Create(
            string email,
            string password,
            string firstName,
            string lastName,
            string? phoneNumber = null)
        {
            // Validate email
            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                return Result.Failure<User>(emailResult.Error);

            // Validate password
            var passwordValidation = ValidatePassword(password);
            if (passwordValidation.IsFailure)
                return Result.Failure<User>(passwordValidation.Error);

            // Validate names
            if (string.IsNullOrWhiteSpace(firstName))
                return Result.Failure<User>("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                return Result.Failure<User>("Last name is required");

            if (firstName.Length > 100)
                return Result.Failure<User>("First name cannot exceed 100 characters");

            if (lastName.Length > 100)
                return Result.Failure<User>("Last name cannot exceed 100 characters");

            // Create password hash
            var passwordHash = PasswordHash.CreateHash(password);

            var user = new User(emailResult.Value, passwordHash, firstName.Trim(), lastName.Trim(), phoneNumber?.Trim());

            return Result.Success(user);
        }

        /// <summary>
        /// Verify password
        /// </summary>
        public bool VerifyPassword(string plainTextPassword)
        {
            return PasswordHash.Verify(plainTextPassword);
        }

        /// <summary>
        /// Record login
        /// </summary>
        public void RecordLogin(DeviceFingerprint deviceFingerprint)
        {
            LastLoginAt = DateTime.UtcNow;

            RaiseDomainEvent(new UserLoggedInEvent(
                Id,
                Email.Value,
                deviceFingerprint.Hash,
                DateTime.UtcNow));
        }

        /// <summary>
        /// Add refresh token
        /// </summary>
        public RefreshToken AddRefreshToken(string token, DateTime expiresAt, DeviceFingerprint deviceFingerprint)
        {
            var refreshToken = RefreshToken.Create(Id, token, expiresAt, deviceFingerprint);
            _refreshTokens.Add(refreshToken);
            return refreshToken;
        }

        /// <summary>
        /// Revoke all refresh tokens for this user
        /// </summary>
        public void RevokeAllRefreshTokens()
        {
            foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
            {
                token.Revoke();
            }
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        private static Result ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result.Failure("Password is required");

            if (password.Length < 8)
                return Result.Failure("Password must be at least 8 characters");

            if (password.Length > 100)
                return Result.Failure("Password cannot exceed 100 characters");

            if (!password.Any(char.IsDigit))
                return Result.Failure("Password must contain at least one number");

            if (!password.Any(char.IsUpper))
                return Result.Failure("Password must contain at least one uppercase letter");

            if (!password.Any(char.IsLower))
                return Result.Failure("Password must contain at least one lowercase letter");

            return Result.Success();
        }

        /// <summary>
        /// Verify email
        /// </summary>
        public void VerifyEmail()
        {
            IsEmailVerified = true;
            EmailVerifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivate account
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
