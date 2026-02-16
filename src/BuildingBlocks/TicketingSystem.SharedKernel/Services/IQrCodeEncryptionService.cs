using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Services
{
    /// <summary>
    /// Provides AES-256-GCM authenticated encryption for QR code payloads.
    /// Implemented once in SharedKernel; consumed by both Fulfillment (encrypt)
    /// and Access (decrypt) modules.
    /// </summary>
    public interface IQrCodeEncryptionService
    {
        /// <summary>
        /// Encrypts a plain-text QR payload. Returns a Base64-encoded string
        /// containing the random nonce + authentication tag + ciphertext.
        /// Cannot fail under normal operation.
        /// </summary>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts and authenticates a QR payload previously encrypted by Encrypt().
        /// Returns Result.Failure if the ciphertext is malformed, tampered, or
        /// encrypted with a different key — caller maps this to DenialReason.InvalidTicket.
        /// </summary>
        Result<string> Decrypt(string cipherText);
    }
}
