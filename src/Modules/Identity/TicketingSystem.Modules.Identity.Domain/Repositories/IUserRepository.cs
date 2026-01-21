using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Modules.Identity.Domain.Entities;
using TicketingSystem.Modules.Identity.Domain.ValueObjects;
using TicketingSystem.SharedKernel;

namespace TicketingSystem.Modules.Identity.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Get user by email
        /// </summary>
        Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if email already exists
        /// </summary>
        Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user with refresh tokens
        /// </summary>
        Task<User?> GetByIdWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user by refresh token
        /// </summary>
        Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
