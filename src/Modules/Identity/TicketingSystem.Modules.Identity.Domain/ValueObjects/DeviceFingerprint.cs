using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.ValueObjects
{
     ///<summary>
/// Device fingerprint for tracking user devices
/// </summary>
    public class DeviceFingerprint : ValueObject
    {
        public string Hash { get; private set; }
        public string UserAgent { get; private set; }
        public string IpAddress { get; private set; }

        private DeviceFingerprint(string hash, string userAgent, string ipAddress)
        {
            Hash = hash;
            UserAgent = userAgent;
            IpAddress = ipAddress;
        }

        public static DeviceFingerprint Create(string userAgent, string ipAddress, string? additionalData = null)
        {
            userAgent = userAgent ?? "Unknown";
            ipAddress = ipAddress ?? "Unknown";

            // Combine data and hash it
            var combinedData = $"{userAgent}|{ipAddress}|{additionalData}";
            var hash = ComputeHash(combinedData);

            return new DeviceFingerprint(hash, userAgent, ipAddress);
        }

        private static string ComputeHash(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Hash;
        }
    }
}
