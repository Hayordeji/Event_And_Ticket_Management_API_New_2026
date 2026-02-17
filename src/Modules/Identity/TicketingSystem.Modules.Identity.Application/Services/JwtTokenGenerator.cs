using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

//using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Infrastructure.Persistence.Configurations;

namespace TicketingSystem.Modules.Identity.Application.Services
{
    ///<summary>
/// JWT token generator implementation
/// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtConfig _config;

        public JwtTokenGenerator(IOptions<JwtConfig> config)
        {
            _config = config.Value;
        }

        public string GenerateAccessToken(User user, string role)
        {
            var claims = new[]
            {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, role),
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.Secret));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_config.ExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
