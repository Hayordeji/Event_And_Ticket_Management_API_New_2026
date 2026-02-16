using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Entities
{
    public class RefreshToken 
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }

        public string Token { get; private set; } = string.Empty;
        public string DeviceInfo { get; private set; } = string.Empty;

        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsActive => !IsExpired && !IsRevoked;

        private RefreshToken() { } // EF Core

        public static RefreshToken Create(
            Guid userId,
            string deviceInfo,
            int expiryDays = 7)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = GenerateSecureToken(),
                DeviceInfo = deviceInfo,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Revoke()
        {
            if (IsRevoked) return; // Idempotent
            RevokedAt = DateTime.UtcNow;
        }

        private static string GenerateSecureToken()
        {
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
