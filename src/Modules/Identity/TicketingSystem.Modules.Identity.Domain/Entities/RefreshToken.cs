using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Entities
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string DeviceFingerprintHash { get; private set; } = string.Empty;
        public string UserAgent { get; private set; } = string.Empty;
        public string IpAddress { get; private set; } = string.Empty;

        // EF Core constructor
        private RefreshToken() { }

        private RefreshToken(
            Guid userId,
            string token,
            DateTime expiresAt,
            DeviceFingerprint deviceFingerprint)
            : base()
        {
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            IsRevoked = false;
            DeviceFingerprintHash = deviceFingerprint.Hash;
            UserAgent = deviceFingerprint.UserAgent;
            IpAddress = deviceFingerprint.IpAddress;
        }

        public static RefreshToken Create(
            Guid userId,
            string token,
            DateTime expiresAt,
            DeviceFingerprint deviceFingerprint)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty", nameof(token));

            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiration must be in the future", nameof(expiresAt));

            return new RefreshToken(userId, token, expiresAt, deviceFingerprint);
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke()
        {
            if (IsRevoked)
                return;

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }
    }
}
