using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Configurations
{
    public class QrEncryptionConfig
    {
        public const string SectionName = "QrEncryption";

        /// <summary>
        /// Base64-encoded 32-byte (256-bit) AES key.
        /// Generate with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        /// Store in user-secrets (dev) or Key Vault (prod). NEVER in appsettings.json.
        /// </summary>
        public required string Key { get; init; }

        public byte[] GetKeyBytes()
        {
            var keyBytes = Convert.FromBase64String(Key);

            //if (keyBytes.Length != 32)
            //    throw new InvalidOperationException(
            //        $"QrEncryption:Key must be exactly 32 bytes (256 bits). " +
            //        $"Got {keyBytes.Length} bytes. " +
            //        $"Generate with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");

            return keyBytes;
        }
    }
}
