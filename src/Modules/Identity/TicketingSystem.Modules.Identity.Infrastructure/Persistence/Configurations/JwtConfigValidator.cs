using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations
{
    public class JwtConfigValidator : IValidateOptions<JwtConfig>
    {
        public ValidateOptionsResult Validate(string? name, JwtConfig options)
        {
            if (string.IsNullOrWhiteSpace(options.Secret))
                return ValidateOptionsResult.Fail("Jwt:Secret is missing.");

            if (options.Secret.Length < 32)
                return ValidateOptionsResult.Fail(
                    $"Jwt:Secret must be at least 32 characters. Got {options.Secret.Length}.");

            if (string.IsNullOrWhiteSpace(options.Issuer))
                return ValidateOptionsResult.Fail("Jwt:Issuer is missing.");

            if (string.IsNullOrWhiteSpace(options.Audience))
                return ValidateOptionsResult.Fail("Jwt:Audience is missing.");

            if (options.ExpiryMinutes <= 0)
                return ValidateOptionsResult.Fail("Jwt:ExpiryMinutes must be greater than 0.");

            return ValidateOptionsResult.Success;
        }
    }
}
