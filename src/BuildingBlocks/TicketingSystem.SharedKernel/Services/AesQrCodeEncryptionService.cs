using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TicketingSystem.SharedKernel.Configurations;

namespace TicketingSystem.SharedKernel.Services
{
    /// <summary>
    /// AES-256-GCM implementation of IQrCodeEncryptionService.
    ///
    /// Wire format (Base64 of concatenated bytes):
    ///   [12 bytes: random nonce] [16 bytes: GCM auth tag] [N bytes: ciphertext]
    ///
    /// GCM authentication tag guarantees both confidentiality AND integrity.
    /// A tampered byte anywhere in the payload causes Decrypt() to return Failure —
    /// no additional HMAC is needed.
    /// </summary>
    public class AesQrCodeEncryptionService : IQrCodeEncryptionService
    {
        // AES-GCM standard sizes — do not change without a full migration plan.
        private const int NonceSizeBytes = 12; // 96-bit nonce (NIST recommendation for GCM)
        private const int TagSizeBytes = 16;   // 128-bit authentication tag

        private readonly byte[] _key;

        public AesQrCodeEncryptionService(IOptions<QrEncryptionConfig> config)
        {
            _key = config.Value.GetKeyBytes(); // Throws at startup if key is malformed — fail fast
        }

        /// <inheritdoc />
        public string Encrypt(string plainText)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes); // Cryptographically random nonce

            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[TagSizeBytes];

            using var aes = new AesGcm(_key, TagSizeBytes);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            // Layout: nonce (12) | tag (16) | ciphertext (N)
            // Prepending nonce & tag allows stateless decryption — no lookup table needed.
            var combined = new byte[NonceSizeBytes + TagSizeBytes + cipherBytes.Length];
            nonce.CopyTo(combined, 0);
            tag.CopyTo(combined, NonceSizeBytes);
            cipherBytes.CopyTo(combined, NonceSizeBytes + TagSizeBytes);

            return Convert.ToBase64String(combined);
        }

        /// <inheritdoc />
        public Result<string> Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return Result.Failure<string>("QR code payload is empty.");

            try
            {
                var combined = Convert.FromBase64String(cipherText);

                // Minimum length check: nonce + tag must be present
                var minimumLength = NonceSizeBytes + TagSizeBytes;
                if (combined.Length <= minimumLength)
                    return Result.Failure<string>("QR code payload is too short to be valid.");

                var nonce = combined.AsSpan(0, NonceSizeBytes);
                var tag = combined.AsSpan(NonceSizeBytes, TagSizeBytes);
                var cipherBytes = combined.AsSpan(NonceSizeBytes + TagSizeBytes);

                var plainBytes = new byte[cipherBytes.Length];

                using var aes = new AesGcm(_key, TagSizeBytes);
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
                // ↑ Throws CryptographicException if tag verification fails (tampered data)

                return Result.Success(Encoding.UTF8.GetString(plainBytes));
            }
            catch (FormatException)
            {
                // Invalid Base64 — not a valid QR code from this system
                return Result.Failure<string>("QR code contains invalid data.");
            }
            catch (CryptographicException)
            {
                // GCM tag mismatch — ciphertext was tampered with, or wrong key
                return Result.Failure<string>("QR code authentication failed. The code may have been tampered with.");
            }
        }
    }
}
