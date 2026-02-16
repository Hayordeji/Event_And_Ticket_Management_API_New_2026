using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.SharedKernel.Configurations
{
    public class QrEncryptionConfigValidator : IValidateOptions<QrEncryptionConfig>
    {
        public ValidateOptionsResult Validate(string? name, QrEncryptionConfig options)
        {
            if (string.IsNullOrWhiteSpace(options.Key))
                return ValidateOptionsResult.Fail(
                    "QrEncryption:Key is missing. " +
                    "Generate one with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");

            try
            {
                var keyBytes = Convert.FromBase64String(options.Key);

                if (keyBytes.Length != 32)
                    return ValidateOptionsResult.Fail(
                        $"QrEncryption:Key must be exactly 32 bytes (256-bit). " +
                        $"Got {keyBytes.Length} bytes.");
            }
            catch (FormatException)
            {
                return ValidateOptionsResult.Fail(
                    "QrEncryption:Key is not valid Base64. " +
                    "Generate one with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
    